using AspIdentity.Stores.FileStore;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;

namespace AspIdentity.Stores.Test.FileStore
{
    public class UserManagerTest
    {
        public const string FILE_PATH = "./Store.txt";

        [Fact]
        public void UserManager_Create()
        {
            try
            {
                UserManager<IdentityUser> testManager = new UserManager<IdentityUser>(new FileUserStore<IdentityUser>(FILE_PATH));
                var createUser = new IdentityUser()
                {
                    Id = "user1",
                    UserName = "Login"
                };
                var result = testManager.Create(createUser);
                var text = File.ReadAllText(FILE_PATH);

                Assert.True(result.Succeeded);
                Assert.Equal("{\"Id\":\"user1\",\"UserName\":\"Login\",\"Age\":0} PASSWORD_FOR_USER: \r\n", text);
            }
            finally
            {
                removeFile();
            }
        }

        [Fact]
        public void UserManager_Create_SetPassword()
        {
            try
            {
                UserManager<IdentityUser> testManager = new UserManager<IdentityUser>(new FileUserStore<IdentityUser>(FILE_PATH));
                var createUser = new IdentityUser()
                {
                    Id = "user1",
                    UserName = "Login"
                };
                var result = testManager.Create(createUser, "password1");
                Assert.True(result.Succeeded);

                var fileContent = File.ReadAllText(FILE_PATH);
                Assert.StartsWith(
                    "{\"Id\":\"user1\",\"UserName\":\"Login\",\"Age\":0} PASSWORD_FOR_USER: ",
                    fileContent);

                Assert.NotEmpty(
                   fileContent.Replace(
                       "{\"Id\":\"user1\",\"UserName\":\"Login\",\"Age\":0} PASSWORD_FOR_USER: ",
                       "").Trim());
            }
            finally
            {
                removeFile();
            }
        }

        [Fact]
        public void UserManager_Delete()
        {
            try
            {
                File.WriteAllText(
                    FILE_PATH,
                    "{\"Id\":\"user1\",\"UserName\":\"Login\"} PASSWORD_FOR_USER: AFZrIXBeoXEXpHgp43TBfBdl5JaHF9urD07PKiqR7y1ZdNMJwNgtbID2JfhI87HyDA=="
);

                UserManager<IdentityUser> testManager = new UserManager<IdentityUser>(new FileUserStore<IdentityUser>(FILE_PATH));
                var createUser = new IdentityUser()
                {
                    Id = "user1",
                    UserName = "Login"
                };
                var result = testManager.Delete(createUser);
                Assert.True(result.Succeeded);
                Assert.Empty(File.ReadAllBytes(FILE_PATH));
            }
            finally
            {
                removeFile();
            }
        }


        [Fact]
        public void UserManager_Update()
        {
            try
            {
                File.WriteAllText(
                    FILE_PATH,
                    "{\"Id\":\"user1\",\"UserName\":\"Login\"} PASSWORD_FOR_USER: AFZrIXBeoXEXpHgp43TBfBdl5JaHF9urD07PKiqR7y1ZdNMJwNgtbID2JfhI87HyDA==");

                UserManager<IdentityUser> testManager = new UserManager<IdentityUser>(new FileUserStore<IdentityUser>(FILE_PATH));
                var updateUser = new IdentityUser()
                {
                    Id = "user1",
                    Age = 12,
                    UserName = "Login"
                };
                var result = testManager.Update(updateUser);

                Assert.True(result.Succeeded);
                var fileContent = File.ReadAllText(FILE_PATH);
                Assert.Equal(
                    "{\"Id\":\"user1\",\"UserName\":\"Login\",\"Age\":12} PASSWORD_FOR_USER: AFZrIXBeoXEXpHgp43TBfBdl5JaHF9urD07PKiqR7y1ZdNMJwNgtbID2JfhI87HyDA==\r\n",
                fileContent);
            }
            finally
            {
                removeFile();
            }
        }

        [Fact]
        public void UserManager_Find_ByLoginAndPassword()
        {
            try
            {
                File.WriteAllText(
                    FILE_PATH,
                    "{\"Id\":\"user1\",\"UserName\":\"Login\"} PASSWORD_FOR_USER: AKWQFUhI3nwRgSlTGpZNfhFVGz104+L6IY9Zv4zqpT8THn4NQxqWHeAQ+/YkvMxjzg=="
                    );

                UserManager<IdentityUser> testManager = new UserManager<IdentityUser>(
                    new FileUserStore<IdentityUser>(FILE_PATH));
                var result = testManager.Find(
                    "Login",
                    "password1");

                Assert.NotNull(result);
                Assert.Equal("user1", result.Id);
                Assert.Equal("Login", result.UserName);
            }
            finally
            {
                removeFile();
            }
        }

        [Fact]
        public void UserManager_CheckPassword()
        {
            try
            {
                File.WriteAllText(
                    FILE_PATH,
                    "{\"Id\":\"user1\",\"UserName\":\"Login\"} PASSWORD_FOR_USER: AFZrIXBeoXEXpHgp43TBfBdl5JaHF9urD07PKiqR7y1ZdNMJwNgtbID2JfhI87HyDA=="
                    );

                UserManager<IdentityUser> testManager = new UserManager<IdentityUser>(
                    new FileUserStore<IdentityUser>(FILE_PATH));
                var checkPassword = new IdentityUser()
                {
                    Id = "user1",
                    UserName = "Login"
                };
                var result = testManager.CheckPassword(
                    checkPassword,
                    "password1");

                Assert.True(result);
            }
            finally
            {
                removeFile();
            }
        }

        private void removeFile()
        {
            for (int i = 0; i < 100; i++)
                try
                {
                    System.IO.File.Delete(FILE_PATH);
                }
                catch (IOException)
                {
                    Thread.Sleep(1);
                }
        }

    }
}
