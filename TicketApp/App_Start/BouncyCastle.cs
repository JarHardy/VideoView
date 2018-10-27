using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Net;
using System.Data;
using System.Web.Script.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using System.Collections;
using Org.BouncyCastle.Asn1.Pkcs;
using System.Xml;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;


namespace MyAngularApp
{

    /**
   * Interface for the OCSP Client.
   * @since 2.1.6
   */
    public interface IOcspClient
    {
        /**
        * Gets an encoded byte array.
        * @return   a byte array
        */
        byte[] GetEncoded();
    }

    /**
    * OcspClient implementation using BouncyCastle.
    * @author psoares
    * @since  2.1.6
    */

    public class OcspClientBouncyCastle : IOcspClient
    {
        /** root certificate */
        private Org.BouncyCastle.X509.X509Certificate rootCert;
        /** check certificate */
        private Org.BouncyCastle.X509.X509Certificate checkCert;
        /** OCSP URL */
        private String url;

        /**
        * Creates an instance of an OcspClient that will be using BouncyCastle.
        * @param checkCert  the check certificate
        * @param rootCert  the root certificate
        * @param url  the OCSP URL
        */
        public OcspClientBouncyCastle(Org.BouncyCastle.X509.X509Certificate checkCert, Org.BouncyCastle.X509.X509Certificate rootCert, String url)
        {
            this.checkCert = checkCert;
            this.rootCert = rootCert;
            this.url = url;
        }

        /**
        * Generates an OCSP request using BouncyCastle.
        * @param issuerCert  certificate of the issues
        * @param serialNumber  serial number
        * @return  an OCSP request
        * @throws OCSPException
        * @throws IOException
        */
        private static OcspReq GenerateOCSPRequest(Org.BouncyCastle.X509.X509Certificate issuerCert, BigInteger serialNumber)
        {
            // Generate the id for the certificate we are looking for
            CertificateID id = new CertificateID(CertificateID.HashSha1, issuerCert, serialNumber);

            // basic request generation with nonce.
            // a nonce is generated from cryptographic random number generators. 
            OcspReqGenerator gen = new OcspReqGenerator();

            gen.AddRequest(id);

            byte[] sampleNonce = new byte[16];
            Random rand = new Random();
            rand.NextBytes(sampleNonce);

            // create details for nonce extension
            ArrayList oids = new ArrayList();
            ArrayList values = new ArrayList();
            oids.Add(OcspObjectIdentifiers.PkixOcspNonce);
            values.Add(new Org.BouncyCastle.Asn1.X509.X509Extension(false, new DerOctetString(sampleNonce)));
            gen.SetRequestExtensions(new X509Extensions(oids, values));

            // Generate request
            var req = gen.Generate();

            // is the request signed?
            if (req.IsSigned)
            {
                Debug.WriteLine("is signed!");
            }

            Org.BouncyCastle.X509.X509Certificate[] certs = req.GetCerts();

            // Check if certs are not null
            if (certs != null)
            {
                Debug.WriteLine("No certs!");
            }

            Req[] requests = req.GetRequestList();

            if (!requests[0].GetCertID().Equals(id))
            {
                Debug.WriteLine("id not found!");
            }


            return req;
        }

        public byte[] GetEncoded() { return null; }

        /**
        * @return   a byte array
        * @see com.lowagie.text.pdf.OcspClient#getEncoded()
        */
        public Boolean runAuth()
        {
            OcspReq request = GenerateOCSPRequest(rootCert, checkCert.SerialNumber);

            //    Debug.WriteLine(checkCert.SerialNumber.ToString(16));
            Debug.WriteLine("..running OCSP check with : " + url);

            byte[] array = request.GetEncoded();

            // foreach (var i in array) { Debug.WriteLine(Convert.ToBase64String(i)); }
            HttpWebRequest con = (HttpWebRequest)WebRequest.Create(url);
            con.ContentLength = array.Length;
            con.ContentType = "application/ocsp-request";
            con.Accept = "application/ocsp-response";
            con.Method = "POST";
            Stream outp;
            try
            {
                outp = con.GetRequestStream();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception : " + e.Message);
                return false;
            }

            outp.Write(array, 0, array.Length);
            outp.Close();

            HttpWebResponse response = (HttpWebResponse)con.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK) throw new IOException("invalid.http.response.1" + response.StatusCode);

            Stream inp = response.GetResponseStream();
            OcspResp ocspResponse = new OcspResp(inp);
            string responseText;
            using (var reader = new System.IO.StreamReader(inp, ASCIIEncoding.ASCII))
            {
                responseText = reader.ReadToEnd();
            }
            inp.Close();
            response.Close();



            if (ocspResponse.Status != 0) throw new IOException("invalid.status.1" + ocspResponse.Status);
            BasicOcspResp basicResponse = (BasicOcspResp)ocspResponse.GetResponseObject();

            var resp_certs = basicResponse.GetCerts();
            //basicResponse.GetCertificates("Collection");            

            X509Store store = new X509Store(StoreName.CertificateAuthority);
            store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly);

            int num_matches = 0;
            foreach (var c in resp_certs)
            {
                // Debug.WriteLine("...");
                // cehck subject or issuer to see if in store
                // Debug.WriteLine(c.SubjectDN);
                // Debug.WriteLine(c.IssuerDN);

                string issuer_cn = c.IssuerDN.ToString().Split(new string[] { "CN=" }, StringSplitOptions.None)[1].Split(',')[0];
                var fndCA = store.Certificates.Find(X509FindType.FindBySubjectName, issuer_cn, true);
                if (fndCA.Count > 0) num_matches++;
            }

            if (num_matches != resp_certs.Length)
            {
                throw new IOException("Response certificate validation failed!");
            }

            if (basicResponse != null)
            {
                SingleResp[] responses = basicResponse.Responses;
                if (responses.Length == 1)
                {
                    SingleResp resp = responses[0];

                    Object status = resp.GetCertStatus();

                    // Debug.WriteLine(status+"=?"+CertificateStatus.Good);

                    if (status == CertificateStatus.Good)
                    {
                        //Debug.WriteLine("CERT IS GOOD!! VALID!!");
                        //return basicResponse.GetEncoded();
                        return true;
                    }
                    else if (status is Org.BouncyCastle.Ocsp.RevokedStatus)
                    {
                        //throw new IOException("ocsp.status.is.revoked");
                        Debug.WriteLine("Cert is revoked!");
                        return false;
                    }
                    else
                    {
                        //Debug.WriteLine(responseText);
                        //throw new IOException("ocsp.status.is.unknown ");
                        Debug.WriteLine("Unknown status!");
                        return false;
                    }
                }

                else
                {

                    Debug.WriteLine("DID NOT GET UNIQUE RESPONSE! (" + responses.Length + ")");
                    /*
                    foreach (SingleResp r in responses)
                    {
                        Debug.WriteLine("..." + r.GetCertID()+" :: "+r.GetCertStatus());
                    }*/
                }
            }
            else
            {
                Debug.WriteLine("BASIC RESPONSE WAS NULL!");
            }
            return false;
        }
    }



    public class BouncyCastle : Controller
    {

        public partial class ActiveUser
        {
            public int rrc_edipi { get; set; }
            public int edipi { get; set; }
            public string lastName { get; set; }
            public string firstName { get; set; }
            public string middleInitial { get; set; }
            public string email { get; set; }
            public string rank { get; set; }
            public string dsn_phone { get; set; }
            public string alt_phone { get; set; }
            public string macom { get; set; }
            public string base_location { get; set; }
            public string state { get; set; }
            public string country_cd { get; set; }
            public string status { get; set; }
        }



        //public static string ResolveRelativePath(string referencePath, string relativePath)
        //{
        //    Uri uri = new Uri(Path.Combine(referencePath, relativePath));
        //    return Path.GetFullPath(uri.AbsolutePath);
        //}

        //private static Dictionary<string, string> api_site_tokens = new Dictionary<string, string>();


        //private static int logID = -1;

        public ActiveUser setUser()
        {
            Debug.WriteLine("here in CoreApp.setUser");

            ActiveUser active_user = new ActiveUser();
            HttpClientCertificate cert = Request.ClientCertificate;

            //start of mikes code 
            String email = null;

            System.Security.Cryptography.X509Certificates.X509Certificate ucert = new System.Security.Cryptography.X509Certificates.X509Certificate(cert.Certificate);
            var ucert1 = Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(ucert);

            Regex email_pat = new Regex(@"^.*@*\.mil$", RegexOptions.IgnoreCase);   // Search for an email string
            var subject_alt_names = ucert1.GetSubjectAlternativeNames();                           // Get subject alternative names from cert using and store into a collection
            if (subject_alt_names != null)					                         // if collection is not null
            {
                foreach (var k in subject_alt_names)					     // here is where we hit the root of the collection using object 'k' to iterate through the collection
                {
                    foreach (var i in (ArrayList)k)						  // going deeper into the child elements of object k.... typecast to an arraylist
                    {
                        Match m = email_pat.Match(Convert.ToString(i));			 // use system api Match and see if what we are looking for is an email..... 
                        if (m.Success)
                        {
                            email = Convert.ToString(i);					 // convert object to string and set email equal to the matched value
                            Debug.WriteLine("Subject Alternative Name email: " + email);
                            break;
                        }
                    }
                }
            }

            //end of mikes code. go down to the return value from here


            if (cert.IsPresent)
            {
                if (Request.RequestContext.HttpContext.Session["ocsp_checked"] == null)
                {
                    X509Store store = new X509Store(StoreName.Root);
                    store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly);


                    bool ocsp_testing_phase = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["ocsp_testing"]);
                    System.Security.Cryptography.X509Certificates.X509Certificate user_cert;
                    if (ocsp_testing_phase)
                    {// JUST FOR TESTING
                        ArrayList test_users = new ArrayList() { "AnVLAuthUser1.cer", "AnVLAuthUser2.cer", "amrdec_ocsp_test\\david.kalpakchian.ctr_base64.cer", "localhost_cert.cer" };
                        user_cert = new System.Security.Cryptography.X509Certificates.X509Certificate();

                        // throw new IOException("HERE - " + HttpContext.Current.Server.MapPath(".") + "\n" + System.IO.Directory.GetCurrentDirectory() + "\n" + Path.GetDirectoryName(HttpContext.Current.Server.MapPath(".")) + "\n" + Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()) + "\n" + ResolveRelativePath(HttpContext.Current.Server.MapPath("."), "..\\App_Data\\" + test_users[0]));

                        //  user_cert.Import("..\\App_Data\\" + test_users[0]);
                        user_cert.Import(AppDomain.CurrentDomain.BaseDirectory + "Data\\" + test_users[0]); //ResolveRelativePath(HttpContext.Current.Server.MapPath("."), "..\\Data\\" + test_users[0]));


                    }
                    else
                    {
                        // real cert of user
                        user_cert = new System.Security.Cryptography.X509Certificates.X509Certificate(cert.Certificate);
                        Debug.WriteLine("\n\nUsing real certificate for OCSP! " + user_cert.GetExpirationDateString() + "\n\n");
                    }

                    var exp_date = DateTime.Parse(user_cert.GetExpirationDateString());

                    if ((exp_date - DateTime.Now).TotalMilliseconds < 0)
                    {
                        Request.RequestContext.HttpContext.Session["ocsp_cert_good"] = false;
                        active_user.edipi = -1;
                        return active_user;
                    }

                    string issuer_cn = user_cert.Issuer.ToString().Split(new string[] { "CN=" }, StringSplitOptions.None)[1].Split(',')[0];

                    var fndCA = store.Certificates.Find(X509FindType.FindBySubjectName, issuer_cn, true);  // (ocsp_testing_phase) ? "DOD JITC CA-27" : issuer_cn

                    if (fndCA.Count == 0)
                    {
                        store = new X509Store(StoreName.CertificateAuthority); // intermediate CAs
                        store.Open(System.Security.Cryptography.X509Certificates.OpenFlags.ReadOnly);

                        fndCA = store.Certificates.Find(X509FindType.FindBySubjectName, issuer_cn, true);
                    }


                    if (fndCA.Count == 0)
                    {
                        throw new IOException("Could not find the appropriate issuer certificate!");
                    }

                    System.Security.Cryptography.X509Certificates.X509Certificate2 rootCA = fndCA[0];

                    //  Debug.WriteLine(rootCA.Subject);
                    bool is_sipr = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["is_sipr"]);

                    bool chk_subject = false;

                    // print diagnostic information to the server log
                    Response.AppendToLog("====**** CERT SUBJECT : " + rootCA.Subject + " ******======");

                    chk_subject = rootCA.Subject.Contains("OU=" + ((is_sipr) ? "DoD" : "PKI")) && rootCA.Subject.Contains("O=U.S. Government") && rootCA.Subject.Contains("C=US");



                    if (!chk_subject) throw new IOException("Could not validate issuing CA!");


                    string ocsp_url = (ocsp_testing_phase) ? System.Configuration.ConfigurationManager.AppSettings["ocsp_responder_test_url"].ToString() : System.Configuration.ConfigurationManager.AppSettings["ocsp_responder_url"].ToString();


                    var OCSPCheck = new OcspClientBouncyCastle(Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(user_cert), Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(rootCA), ocsp_url);

                    bool do_ocsp = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["do_ocsp"]);
                    var ocsp_resp = (do_ocsp) ? OCSPCheck.runAuth() : true;

                    Request.RequestContext.HttpContext.Session["ocsp_cert_good"] = ocsp_resp;
                    Request.RequestContext.HttpContext.Session["ocsp_checked"] = true;
                }
                else
                {
                    if (!Convert.ToBoolean(Request.RequestContext.HttpContext.Session["ocsp_cert_good"]))
                    {
                        active_user.edipi = -1;
                        return active_user;
                    }
                }


                // clear tokens that are stale (>10min) 

                String subjectcn = cert.Get("SUBJECTCN");
                string sn = cert.SerialNumber;
                int edi = Convert.ToInt32(subjectcn.Substring(subjectcn.LastIndexOf(".") + 1));
                String name = subjectcn.Substring(0, subjectcn.LastIndexOf("."));

                try
                {
                    Response.AppendToLog("====**** " + edi + " : " + name + " ******======");
                }
                catch (Exception err)
                {
                    Debug.WriteLine(err.Message);
                }


                //string sql_q = "select * from hartselleb.cmdr_user where STATUS = 'A' and edipi =" + edi;

                //DataSet dsObj = DBUtils.ExecuteSqlQuery(sql_q);

                //var output = new List<string>();
                //if (dsObj != null && dsObj.Tables[0].Rows.Count == 1)
                //{
                //    active_user = dsObj.Tables[0].AsEnumerable().Select(r => new ActiveUser
                //    {
                //        edipi = Convert.ToInt32(r["EDIPI"]),
                //        rrc_edipi = Convert.ToInt32(r["RRC_EDIPI"]),
                //        lastName = r["LASTNAME"].ToString(),
                //        firstName = r["FIRSTNAME"].ToString(),
                //        middleInitial = r["MIDDLEINITIAL"].ToString(),
                //        email = r["EMAIL"].ToString(),
                //        rank = r["RANK"].ToString(),
                //        dsn_phone = r["DSN_PHONE"].ToString(),
                //        alt_phone = r["ALT_PHONE"].ToString(),
                //        macom = r["MACOM"].ToString(),
                //        base_location = r["BASE"].ToString(),
                //        state = r["STATE"].ToString(),
                //        country_cd = r["COUNTRY_CD"].ToString(),
                //        status = r["STATUS"].ToString()

                //    }).FirstOrDefault();

                //}
                //else
                //{

                //    active_user.edipi = -1;


                //}

            }
            else
            {
                active_user.edipi = -2;
            }


            //adding code mike showed me here
            active_user.email = email;
            //end 

            return active_user;

        }






        public Dictionary<string, object> ExtractPostData(System.Web.HttpRequestBase Request)
        {

            // GET THE POST DATA, PUT INTO JSON/DICTIONARY FORM
            Stream body = Request.InputStream;
            Encoding encoding = Request.ContentEncoding;
            StreamReader reader = new StreamReader(body, encoding);
            string json = reader.ReadToEnd();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var req_data = ser.Deserialize<Dictionary<string, object>>(json);
            return req_data;
        }

        // Bouncy Castle encryption / decryption
        public class BCEngine
        {
            private readonly Encoding _encoding;
            private readonly IBlockCipher _blockCipher;
            private PaddedBufferedBlockCipher _cipher;
            private IBlockCipherPadding _padding;

            public BCEngine(IBlockCipher blockCipher, Encoding encoding)
            {
                _blockCipher = blockCipher;
                _encoding = encoding;
            }

            public void SetPadding(IBlockCipherPadding padding)
            {
                if (padding != null)
                    _padding = padding;
            }

            public string Encrypt(string plain, string key)
            {
                byte[] result = BouncyCastleCrypto(true, _encoding.GetBytes(plain), key);
                return Convert.ToBase64String(result);
            }

            public string Decrypt(string cipher, string key)
            {
                byte[] result = BouncyCastleCrypto(false, Convert.FromBase64String(cipher), key);
                return _encoding.GetString(result);
            }

            private byte[] BouncyCastleCrypto(bool forEncrypt, byte[] input, string key)
            {
                try
                {
                    _cipher = _padding == null ? new PaddedBufferedBlockCipher(_blockCipher) : new PaddedBufferedBlockCipher(_blockCipher, _padding);
                    byte[] keyByte = _encoding.GetBytes(key);
                    _cipher.Init(forEncrypt, new KeyParameter(keyByte));
                    return _cipher.DoFinal(input);
                }
                catch (Org.BouncyCastle.Crypto.CryptoException ex)
                {
                    throw new CryptoException(ex.Message);
                }
            }

        }
        Encoding _encoding = Encoding.ASCII;
        static Pkcs7Padding pkcs = new Pkcs7Padding();
        IBlockCipherPadding _padding = pkcs;
        private string bc_key = "7Lbj/1YNDmkS9mHuAfGlnw==";
        public string AESEncryption(string plain)
        {
            /*
            SecureRandom random = new SecureRandom();
            byte[] keyBytes = new byte[16];
            random.NextBytes(keyBytes);
            string key = Convert.ToBase64String(keyBytes);
            Debug.WriteLine(key);
            */
            BCEngine bcEngine = new BCEngine(new AesEngine(), _encoding);
            bcEngine.SetPadding(_padding);
            return bcEngine.Encrypt(plain, bc_key);
        }

        public string AESDecryption(string cipher)
        {
            BCEngine bcEngine = new BCEngine(new AesEngine(), _encoding);
            bcEngine.SetPadding(_padding);
            return bcEngine.Decrypt(cipher, bc_key);
        }
    }
}
