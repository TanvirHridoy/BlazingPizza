using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingPizza
{
    public class AppUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public UserRole Roles { get; set; }
    }
    public class UserRole
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
