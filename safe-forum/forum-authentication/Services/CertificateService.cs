using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace forum_authentication.Services
{
    public class CertificateService : ICertificateService
    {   
        //TODO: save selfsign and validate username with loged user
        public string SignCsr(string csr, string username)
        {

            using (RSA parent = RSA.Create(4096))
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest parentReq = new CertificateRequest(
                    "CN=Experimental Issuing Authority",
                    parent,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                parentReq.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, true));

                parentReq.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

                using (X509Certificate2 parentCert = parentReq.CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddDays(-45),
                    DateTimeOffset.UtcNow.AddDays(365)))
                {
                    CertificateRequest req = new CertificateRequest(
                        "CN=Valid-Looking Timestamp Authority",
                        rsa,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);

                    req.CertificateExtensions.Add(
                        new X509BasicConstraintsExtension(false, false, 0, false));

                    req.CertificateExtensions.Add(
                        new X509KeyUsageExtension(
                            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
                            false));

                    req.CertificateExtensions.Add(
                        new X509EnhancedKeyUsageExtension(
                            new OidCollection
                            {
                    new Oid("1.3.6.1.5.5.7.3.8")
                            },
                            true));

                    req.CertificateExtensions.Add(
                        new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                    using (X509Certificate2 cert = req.Create(
                        parentCert,
                        DateTimeOffset.UtcNow.AddDays(-1),
                        DateTimeOffset.UtcNow.AddDays(90),
                        new byte[] { 1, 2, 3, 4 }))
                    {
                        // Do something with these certs, like export them to PFX,
                        // or add them to an X509Store, or whatever.
                    }
                }
            }

            return "BEGIN CERTIFICATE XXX";
        }

        //private bool VerifyCsrSubject(string csr, string username)
        //{
        //    System.Security.Cryptography.
        //}




        //private void RunOpenSSL(string command, string stdin = null)
        //{

        //    if()

        //    var dupa = Directory.GetCurrentDirectory() + @"\bin\Debug\netcoreapp3.1\openssl.exe";

        //    var process = new Process()
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = Directory.GetCurrentDirectory() + @"\bin\Debug\netcoreapp3.1\openssl.exe",
        //            Arguments = command,
        //            UseShellExecute = false,
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = string.IsNullOrEmpty(stdin) ? false : true,
        //            RedirectStandardInput = false
        //        }
        //    };
        //    process.Start();

        //    //process.StandardInput

        //    var output = process.StandardOutput.ReadToEnd();
        //    process.WaitForExit();
        //}

        


    }
}

