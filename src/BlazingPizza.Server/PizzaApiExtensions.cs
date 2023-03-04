using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Server;

public static class PizzaApiExtensions
{

    public static WebApplication MapPizzaApi(this WebApplication app)
    {

        // Subscribe to notifications
        app.MapPut("/notifications/subscribe", [Authorize] async (
            HttpContext context,
            PizzaStoreContext db,
            NotificationSubscription subscription) => {

                // We're storing at most one subscription per user, so delete old ones.
                // Alternatively, you could let the user register multiple subscriptions from different browsers/devices.
                var userId = GetUserId(context);
                var oldSubscriptions = db.NotificationSubscriptions.Where(e => e.UserId == userId);
                db.NotificationSubscriptions.RemoveRange(oldSubscriptions);

                // Store new subscription
                subscription.UserId = userId;
                db.NotificationSubscriptions.Attach(subscription);

                await db.SaveChangesAsync();
                return Results.Ok(subscription);

            });

        // Specials
        app.MapGet("/specials", async (PizzaStoreContext db) => {

            var specials = await db.Specials.ToListAsync();
            return Results.Ok(specials);

        });

        // Toppings
        app.MapGet("/toppings", async (PizzaStoreContext db) => {
            var toppings = await db.Toppings.OrderBy(t => t.Name).ToListAsync();
            return Results.Ok(toppings);
        });
        // roles
        app.MapGet("/roles/{UserName}", async (PizzaStoreContext db, string UserName) => {

            var roles = await (from ur in db.UserRoles
                               join u in db.Users on ur.UserId equals u.Id
                               join r in db.Roles on ur.RoleId equals r.Id
                               where u.UserName == UserName
                               select r.Name).ToListAsync();
            return Results.Ok(roles);

        });


        // Role Method
        app.MapGet("/roles", async (PizzaStoreContext db) => {

            var roles = await db.Roles.Select(e => new UserRole
            {
                RoleId = e.Id,
                RoleName = e.Name
            }).ToListAsync();
            return Results.Ok(roles);

        });
        // Payment Method
        app.MapGet("/payment", async (PizzaStoreContext db) => {

            var payMethods = await db.PaymentMethods.ToListAsync();
            return Results.Ok(payMethods);

        });

        app.MapGet("/payment/{id}", async (PizzaStoreContext db, int id) => {

            var payMethod = await db.PaymentMethods.SingleOrDefaultAsync(e => e.Id == id);
            return Results.Ok(payMethod);

        });

        app.MapDelete("/payment/{id}", async (PizzaStoreContext db, int id) => {

            try
            {
                var payMethod = await db.PaymentMethods.SingleOrDefaultAsync(e => e.Id == id);
                db.PaymentMethods.Remove(payMethod);
                await db.SaveChangesAsync();
                return Results.Ok(true);
            }
            catch (Exception EX)
            {
                return Results.Ok(false);
            }


        });

        app.MapPost("/payment", async (PizzaStoreContext db, [FromBody] PaymentMethod method) => {
            try
            {
                if (method != null)
                {
                    if (method.Id > 0)
                    {
                        var data = await db.PaymentMethods.SingleOrDefaultAsync(e => e.Id == method.Id);
                        data.Name = method.Name;
                    }
                    else
                    {
                        await db.PaymentMethods.AddAsync(method);
                    }
                    await db.SaveChangesAsync();
                    return Results.Ok(true);

                }
                return Results.BadRequest(false);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(false);
            }


        });

        // User Method
        app.MapGet("/user", async (PizzaStoreContext db) => {

            var users = await (from u in db.Users
                               join ur in db.UserRoles on u.Id equals ur.UserId into userRoles
                               from ur in userRoles.DefaultIfEmpty()
                               join r in db.Roles on ur.RoleId equals r.Id into roles
                               from r in roles.DefaultIfEmpty()
                               select new AppUser
                               {
                                   UserId = u.Id,
                                   UserName = u.UserName,
                                   Roles = ur == null ? null : new UserRole { RoleId = r.Id, RoleName = r.Name }
                               }).ToListAsync();
            return Results.Ok(users);

        });

        app.MapGet("/user/{id}", async (PizzaStoreContext db, string id) => {

            var user = await (from u in db.Users
                              join ur in db.UserRoles on u.Id equals ur.UserId into userRoles
                              from ur in userRoles.DefaultIfEmpty()
                              join r in db.Roles on ur.RoleId equals r.Id into roles
                              from r in roles.DefaultIfEmpty()
                              where u.Id == id
                              select new AppUser
                              {
                                  UserId = u.Id,
                                  UserName = u.UserName,
                                  Roles = ur == null ? null : new UserRole { RoleId = r.Id, RoleName = r.Name }
                              }).SingleOrDefaultAsync();
            return Results.Ok(user);

        });

        app.MapDelete("/user/{id}", async (PizzaStoreContext db, string id) => {

            try
            {
                var user = await db.Users.SingleOrDefaultAsync(e => e.Id == id);
                if (user != null)
                {
                    if (user != null)
                    {
                        var role = await db.UserRoles.Where(e => e.UserId == user.Id).ToListAsync();
                        db.UserRoles.RemoveRange(role);
                    }
                    db.Users.Remove(user);
                }

                await db.SaveChangesAsync();
                return Results.Ok(true);
            }
            catch (Exception EX)
            {
                return Results.Ok(false);
            }


        });

        app.MapPost("/user", async (PizzaStoreContext db, [FromBody] AppUser model) => {
            try
            {

                if (model != null)
                {

                    var role = await db.UserRoles.Where(e => e.UserId == model.UserId).ToListAsync();
                    db.UserRoles.RemoveRange(role);

                    if (model.Roles != null)
                    {
                        IdentityUserRole<string> uRole = new IdentityUserRole<string>()
                        {
                            RoleId = model.Roles.RoleId,
                            UserId = model.UserId
                        };
                        await db.UserRoles.AddAsync(uRole);
                    }
                    await db.SaveChangesAsync();
                    return Results.Ok(true);

                }
                return Results.BadRequest(false);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(false);
            }


        });
        return app;


    }

    private static string GetUserId(HttpContext context)
    {
        return context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

}