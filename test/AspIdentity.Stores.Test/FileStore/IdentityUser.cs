using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspIdentity.Stores.Test.FileStore
{
    public class IdentityUser : IUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public int Age { get; set; }
    }
}
