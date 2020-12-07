using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace forum_authentication.Services
{
    public interface ICertificateService
    {
        string SignCsr(string csr, string username);
    }
}
