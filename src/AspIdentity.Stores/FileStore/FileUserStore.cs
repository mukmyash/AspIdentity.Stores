using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspIdentity.Stores.FileStore
{
    public class FileUserStore<TUser> : IUserStore<TUser>, IUserPasswordStore<TUser>
        where TUser : class, IUser
    {
        private FileInfo _fileInfo;
        private Dictionary<string, TUser> _users = new Dictionary<string, TUser>();
        private Dictionary<string, string> _userPasswords = new Dictionary<string, string>();

        public FileUserStore(string filePath)
        {
            _fileInfo = new FileInfo(filePath);
            if (!_fileInfo.Exists)
                using (var stream = _fileInfo.Create()) { }
            else
                LoadUsers(filePath);
        }

        private void LoadUsers(string filePath)
        {
            var serializeUsers = File.ReadAllLines(filePath);

            foreach (var serializeUser in serializeUsers)
            {
                var splitString = serializeUser.Split(new string[] { " PASSWORD_FOR_USER: " }, StringSplitOptions.RemoveEmptyEntries);
                if (splitString.Length > 2)
                {
                    throw new Exception("Not valid file format.");
                }

                TUser user = JsonConvert.DeserializeObject<TUser>(splitString[0]);
                if (_users.ContainsKey(user.UserName))
                {
                    throw new Exception($"User \"{user.UserName}\" already exists");
                }
                _users.Add(user.UserName, user);
                _userPasswords.Add(user.UserName, splitString.Length == 2 ? splitString[1] : null);
            }
        }

        public async Task CreateAsync(TUser user)
        {
            if (_users.ContainsKey(user.UserName))
            {
                throw new Exception($"User \"{user.UserName}\" already exists");
            }
            _users.Add(user.UserName, user);
            if (!_userPasswords.ContainsKey(user.UserName))
                _userPasswords.Add(user.UserName, null);

            using (var stream = _fileInfo.AppendText())
            {
                var serializUser = JsonConvert.SerializeObject(user);
                await stream.WriteLineAsync($"{serializUser} PASSWORD_FOR_USER: {_userPasswords[user.UserName]}");
            }
        }

        public async Task DeleteAsync(TUser user)
        {
            if (!_users.ContainsKey(user.UserName))
            {
                throw new Exception($"User \"{user.UserName}\" not found");
            }
            _users.Remove(user.UserName);
            _userPasswords.Remove(user.UserName);
            await RecreateUserFile();
        }

        public void Dispose()
        {

        }

        public async Task<TUser> FindByIdAsync(string userId)
        {
            var result = _users.FirstOrDefault(n => n.Value.Id == userId).Value;
            return await Task.FromResult(result);
        }

        public async Task<TUser> FindByNameAsync(string userName)
        {
            if (!_users.ContainsKey(userName))
                return null;
            return await Task.FromResult(_users[userName]);
        }

        public async Task UpdateAsync(TUser user)
        {
            if (!_users.ContainsKey(user.UserName))
            {
                throw new Exception($"User \"{user.UserName}\" not found");
            }
            _users[user.UserName] = user;
            await RecreateUserFile();
        }

        public async Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            if (!_userPasswords.ContainsKey(user.UserName))
            {
                _userPasswords.Add(user.UserName, passwordHash);
            }
            else
            {
                _userPasswords[user.UserName] = passwordHash;
            }
            await RecreateUserFile();
        }

        public Task<string> GetPasswordHashAsync(TUser user)
        {
            if (!_users.ContainsKey(user.UserName))
            {
                throw new Exception($"User \"{user.UserName}\" not found");
            }
            return Task.FromResult(_userPasswords[user.UserName]);
        }

        public Task<bool> HasPasswordAsync(TUser user)
        {
            if (!_users.ContainsKey(user.UserName))
            {
                throw new Exception($"User \"{user.UserName}\" not found");
            }
            return Task.FromResult(string.IsNullOrEmpty(_userPasswords[user.UserName]));
        }

        private async Task RecreateUserFile()
        {
            StringBuilder serializUsers = new StringBuilder();
            foreach (var _user in _users)
            {
                var password = _userPasswords[_user.Key];
                serializUsers.Append(JsonConvert.SerializeObject(_user.Value));
                serializUsers.Append(" PASSWORD_FOR_USER: ");
                serializUsers.AppendLine(password);
            }
            // File.WriteAllText(_fileInfo.FullName, serializUsers.ToString());

            using (var stream = _fileInfo.CreateText())
            {
                await stream.WriteAsync(serializUsers.ToString());
            }
        }
    }
}
