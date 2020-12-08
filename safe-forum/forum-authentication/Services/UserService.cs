using forum_authentication.Dtos;
using forum_authentication.Entities;
using forum_authentication.Helpers;
using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace forum_authentication.Services
{
    public class UserService : IUserService
    {
        private DataContext _context;

        public UserService(DataContext context)
        {
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Username == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyHash(password, user.PasswordSalt, user.PasswordHash))
                return null;

            // authentication successful
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.ToArray();
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public void Create(UserDto userDto)
        {
            if (_context.Users.Any(x => x.Username == userDto.Username))
                throw new ApplicationException("Username \"" + userDto.Username + "\" is already taken");

            if (VerifyCertificate(userDto.Username, userDto.Certificate))
                throw new ApplicationException(@"At least one of certificate requirements not met: - Subject common name must be identical with username - Algorithm signature: sha256");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            var newUser = new User()
            {
                Certificate = userDto.Certificate,
                Username = userDto.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();
        }

        public void Update(User userParam, string password = null)
        {
            var user = _context.Users.Find(userParam.Id);

            if (user == null)
                throw new ApplicationException("User not found");

            if (userParam.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (_context.Users.Any(x => x.Username == userParam.Username))
                    throw new ApplicationException("Username " + userParam.Username + " is already taken");
            }

            // update user properties
            user.Username = userParam.Username;

            // update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public IEnumerable<string> GetAllUsernames()
        {
            return _context.Users.Select(u => u.Username).ToArray();
        }

        public void UpdateUserCertificate(string username, string certificate)
        {
            var searchUser = _context.Users.SingleOrDefault(u => u.Username == username);
            if(searchUser == null)
            {
                throw new ApplicationException("Username not found");
            }

            var user = new User() {Id = searchUser.Id, Certificate = certificate };
            using (_context)
            {
                _context.Users.Attach(user);
                _context.Entry(user).Property(x => x.Certificate).IsModified = true;
                _context.SaveChanges();
            }
        }

        public string GetUserCertificate(string username)
        {
            var certificate = _context.Users.FirstOrDefault(u => u.Username == username)?.Certificate;
            if(certificate == null)
            {
                throw new ApplicationException("Username not exist");
            }
            return certificate;
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            passwordSalt = CreateSalt();
            passwordHash = HashPassword(password, passwordSalt);
        }

        private static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);
            return buffer;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = 8; // four cores
            argon2.Iterations = 4;
            argon2.MemorySize = 1024 * 1024; // 1 GB

            return argon2.GetBytes(16);
        }

        private static bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash);
        }

        private bool VerifyCertificate(string username, string certificate)
        {
            var decodedCertificate = Encoding.UTF8.GetBytes(certificate);

            var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(decodedCertificate);
            if (x509.Subject != username) return false;
            if (x509.SignatureAlgorithm.FriendlyName != "sha256RSA") return false;
            if (x509.PublicKey.Key.KeySize != 2048) return false;
            return true;
        }


    }
}
