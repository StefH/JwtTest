using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace JwtTest
{
    // based on http://www.jensign.com/opensslkey/opensslkey.cs
    class CertificateFromFileProvider : ICertificateProvider
    {
        // encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1", including the sequence byte and terminal encoded null
        readonly byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

        public X509Certificate2 GetCertificate()
        {
            string certificateText = File.ReadAllText(@"c:\temp\certificate_pub.crt");
            string privateKeyText = File.ReadAllText(@"c:\temp\private.key");

            byte[] certBytes = GetCertificateBytes(certificateText);
            X509Certificate2 certificate = new X509Certificate2(certBytes);

            byte[] privateBytes = GetPrivateBytes(privateKeyText);
            certificate.PrivateKey = DecodePrivateKeyInfo(privateBytes);

            return certificate;
        }

        private byte[] GetPrivateBytes(string text)
        {
            const string header = "-----BEGIN PRIVATE KEY-----";
            const string footer = "-----END PRIVATE KEY-----";
            string data = text.Trim();

            data = data.Replace(header, string.Empty);
            data = data.Replace(footer, string.Empty);

            return Convert.FromBase64String(data.Trim());
        }

        private byte[] GetCertificateBytes(string text)
        {
            const string header = "-----BEGIN CERTIFICATE-----";
            const string footer = "-----END CERTIFICATE-----";
            string data = text.Trim();

            data = data.Replace(header, string.Empty);
            data = data.Replace(footer, string.Empty);

            return Convert.FromBase64String(data.Trim());
        }

        private RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            var memoryStream = new MemoryStream(pkcs8);
            int lenstream = (int)memoryStream.Length;
            var reader = new BinaryReader(memoryStream);

            try
            {
                ushort twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130) // data read as little endian order (actual data order for Sequence is 30 81)
                {
                    reader.ReadByte(); // advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16(); // advance 2 bytes
                }
                else
                {
                    return null;
                }

                byte bt = reader.ReadByte();
                if (bt != 0x02)
                {
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes != 0x0001)
                {
                    return null;
                }

                byte[] seq = reader.ReadBytes(15);
                if (!CompareBytearrays(seq, SeqOID)) // make sure Sequence for OID is correct
                {
                    return null;
                }

                bt = reader.ReadByte();
                if (bt != 0x04) // expect an Octet string 
                {
                    return null;
                }

                bt = reader.ReadByte(); //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                if (bt == 0x81)
                {
                    reader.ReadByte();
                }
                else if (bt == 0x82)
                {
                    reader.ReadUInt16();
                }

                //------ at this stage, the remaining sequence should be the RSA private key
                byte[] rsaprivkey = reader.ReadBytes((int)(lenstream - memoryStream.Position));
                return DecodeRSAPrivateKey(rsaprivkey);
            }
            finally
            {
                reader.Close();
            }
        }

        private RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS;
            byte[] E;
            byte[] D;
            byte[] P;
            byte[] Q;
            byte[] DP;
            byte[] DQ;
            byte[] IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
            var memoryStream = new MemoryStream(privkey);
            var reader = new BinaryReader(memoryStream);
            try
            {
                ushort twobytes = reader.ReadUInt16();
                if (twobytes == 0x8130) // data read as little endian order (actual data order for Sequence is 30 81)
                {
                    reader.ReadByte(); //advance 1 byte
                }
                else if (twobytes == 0x8230)
                {
                    reader.ReadInt16(); // advance 2 bytes
                }
                else
                {
                    return null;
                }

                twobytes = reader.ReadUInt16();
                if (twobytes != 0x0102) // version number
                {
                    return null;
                }

                byte bt = reader.ReadByte();
                if (bt != 0x00)
                {
                    return null;
                }

                //------  all private key components are Integer sequences ----
                int elems = GetIntegerSize(reader);
                MODULUS = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                E = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                D = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                P = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                Q = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                DP = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                DQ = reader.ReadBytes(elems);

                elems = GetIntegerSize(reader);
                IQ = reader.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                var rsaCryptoServiceProvider = new RSACryptoServiceProvider();
                var rsaParameters = new RSAParameters
                {
                    Modulus = MODULUS,
                    Exponent = E,
                    D = D,
                    P = P,
                    Q = Q,
                    DP = DP,
                    DQ = DQ,
                    InverseQ = IQ
                };
                rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                return rsaCryptoServiceProvider;
            }
            finally
            {
                reader.Close();
            }
        }

        private bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }
                i++;
            }

            return true;
        }

        private int GetIntegerSize(BinaryReader reader)
        {
            int count;
            byte bt = reader.ReadByte();
            if (bt != 0x02) // expect integer
            {
                return 0;
            }
            bt = reader.ReadByte();

            if (bt == 0x81)
            {
                count = reader.ReadByte(); // data size in next byte
            }
            else
            {
                if (bt == 0x82)
                {
                    byte highbyte = reader.ReadByte();
                    byte lowbyte = reader.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                {
                    count = bt; // we already have the data size
                }
            }

            while (reader.ReadByte() == 0x00)
            {
                //remove high order zeros in data
                count -= 1;
            }

            reader.BaseStream.Seek(-1, SeekOrigin.Current); // last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }
    }
}