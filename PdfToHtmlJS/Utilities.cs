using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Ionic.Zip;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PdfToHtmlJS
{
    public class Utilities
    {

 
        /// <summary>
        /// To convert ASCII characters to Hexadecimal values. 
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ASCII2Hexadecimal(string word)
        {
            string OutputWord = null;
            for (int k = 0; k < word.Length; ++k)
            {
                char CharacterInWord = word[k];

                //if ((((int)CharacterInWord) > 127) || (((int)CharacterInWord) == 38) || (((int)CharacterInWord) == 60) || (((int)CharacterInWord) == 62))
                if (((int)CharacterInWord) > 127)
                {
                    int ASCIIvalue = System.Convert.ToInt32(CharacterInWord);
                    // Convert the decimal value to a hexadecimal value in string form. 
                    string HexadecimalValue = String.Format("{0:X}", ASCIIvalue);


                    if (HexadecimalValue.Length == 2)
                    {
                        OutputWord = OutputWord + "&#x00" + HexadecimalValue.ToString() + ";";
                    }
                    else if (HexadecimalValue.Length == 3)
                    {
                        OutputWord = OutputWord + "&#x0" + HexadecimalValue.ToString() + ";";
                    }
                    else
                    {
                        OutputWord = OutputWord + "&#x" + HexadecimalValue.ToString() + ";";
                    }

                }
                else
                {
                    OutputWord = OutputWord + CharacterInWord.ToString();
                }
            }
            return OutputWord;
        }


        private string GenerateEncryptedPassword(string password)
        {
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = md5Provider.ComputeHash(passwordBytes);
            System.Text.StringBuilder encryptedPassword = new System.Text.StringBuilder();
            foreach (byte pByte in passwordBytes)
            {
                encryptedPassword.Append(pByte.ToString("x2").ToLower());
            }
            return encryptedPassword.ToString();
        }

        /// <summary>
        /// Converts no.to Roman
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("Page no. should be between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "m" + ToRoman(number - 1000);
            if (number >= 900) return "cm" + ToRoman(number - 900);
            if (number >= 500) return "d" + ToRoman(number - 500);
            if (number >= 400) return "cd" + ToRoman(number - 400);
            if (number >= 100) return "c" + ToRoman(number - 100);
            if (number >= 90) return "xc" + ToRoman(number - 90);
            if (number >= 50) return "l" + ToRoman(number - 50);
            if (number >= 40) return "xl" + ToRoman(number - 40);
            if (number >= 10) return "x" + ToRoman(number - 10);
            if (number >= 9) return "ix" + ToRoman(number - 9);
            if (number >= 5) return "v" + ToRoman(number - 5);
            if (number >= 4) return "iv" + ToRoman(number - 4);
            if (number >= 1) return "i" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }


        /// <summary>
        /// Executes a shell command synchronously.
        /// Use this overloaded method if command have path with space(s).
        /// </summary>
        /// <param name="command">string command</param>
        /// <param name="argument">string argument</param>
        /// <returns>string, as output of the command.</returns>
        public static string ExecuteCommandSync(string command, string argument)
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = command;
                proc.StartInfo.Arguments = argument;
                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                // Do not create the black window.
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();

                // Get the output into a string
                var result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
                return result;
            }
            catch (Exception objException)
            {
               // Logger.Write(objException);
                return string.Empty;
            }
            finally
            {
                if (proc != null)
                {
                    proc.Close();
                    proc.Dispose();
                }
            }
        }
       
        /// <summary>
        /// Copy directory to destination folder
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strDestination"></param>
        public static void CopyDirectory(string strSource, string strDestination)
        {
            if (!Directory.Exists(strDestination))
            {
                Directory.CreateDirectory(strDestination);
            }
            DirectoryInfo dirInfo = new DirectoryInfo(strSource);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo tempfile in files)
            {
                tempfile.CopyTo(Path.Combine(strDestination, tempfile.Name));
            }
            DirectoryInfo[] dirctories = dirInfo.GetDirectories();
            foreach (DirectoryInfo tempdir in dirctories)
            {
                CopyDirectory(Path.Combine(strSource, tempdir.Name), Path.Combine(strDestination, tempdir.Name));
            }
        }


        /// <summary>
        /// Copy directory to destination folder
        /// </summary>
        /// <param name="strSource"></param>
        /// <param name="strDestination"></param>
        public static void MoveDirectory(string strSource, string strDestination)
        {
            if (!Directory.Exists(strDestination))
            {
                Directory.CreateDirectory(strDestination);
            }
            DirectoryInfo dirInfo = new DirectoryInfo(strSource);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo tempfile in files)
            {
                tempfile.MoveTo(Path.Combine(strDestination, tempfile.Name));
            }
            DirectoryInfo[] dirctories = dirInfo.GetDirectories();
            foreach (DirectoryInfo tempdir in dirctories)
            {
                CopyDirectory(Path.Combine(strSource, tempdir.Name), Path.Combine(strDestination, tempdir.Name));
            }

        }

     

        /// <summary>
        /// Delete an existing directory
        /// </summary>
        /// <param name="targetDir"></param>
        public static void DeleteDirectory(string targetDir)
        {
            try
            {
                var directory = new DirectoryInfo(targetDir);
                foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
                foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(5);
                DeleteDirectory(targetDir);
            }
        }

        public static void MoveFile(string oldLocation, string newLocation)
        {
            var parentDir = Path.GetDirectoryName(newLocation);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }
            File.Move(oldLocation, newLocation);
        }

        /// <summary>
        /// Decompress zip file with help of ionic.dll
        /// </summary>
        /// <param name="ZipFilePath"></param>
        /// <param name="TargetPath"></param>
        /// <param name="Password"></param>
        /// <param name="OverwriteExistingFiles"></param>
        /// <returns></returns>
        public string Decompress(string ZipFilePath, string TargetPath, bool OverwriteExistingFiles)
        {
            try
            {
                using (ZipFile decompress = ZipFile.Read(ZipFilePath))
                {
                    foreach (ZipEntry e in decompress)
                    {
                        e.Extract(TargetPath, OverwriteExistingFiles ? ExtractExistingFileAction.OverwriteSilently : ExtractExistingFileAction.DoNotOverwrite);
                    }
                }
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public bool Compress(string convertedBook, string TargetPath)
        {
            try
            {
                using (ZipFile zipFile = new ZipFile())
                {
                    zipFile.AddDirectory(convertedBook);
                    zipFile.Save(TargetPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

 

  

        /// <summary>
        /// Send subscription mail.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="userName"></param>
        /// <param name="firstName"></param>
        /// <param name="middleName"></param>
        /// <param name="lastName"></param>
        /// <param name="toEmailAddress"></param>
        /// <param name="fromEmailAddress"></param>
        public static void SendSubscriptionMail(string subject, string userName, string firstName,string middleName,string lastName, string toEmailAddress, string fromEmailAddress)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(string.Concat(AppDomain.CurrentDomain.BaseDirectory, "HtmlMails\\ConversionStatus.html"));
                if (reader != null)
                {
                    string readFile = reader.ReadToEnd();
                    string customisedMail = string.Empty;
                    customisedMail = readFile;
                    customisedMail = customisedMail.Replace("$$UserName$$", string.IsNullOrEmpty(userName) ? "" : userName);
                    customisedMail = customisedMail.Replace("$$FirstName$$", string.IsNullOrEmpty(firstName) ? "" : firstName);
                    customisedMail = customisedMail.Replace("$$MiddleName$$", string.IsNullOrEmpty(middleName) ? "" : middleName);
                    customisedMail = customisedMail.Replace("$$LastName$$", string.IsNullOrEmpty(lastName) ? "" : lastName);
                    customisedMail = customisedMail.Replace("$$EmailAddress$$", string.IsNullOrEmpty(toEmailAddress) ? "" : toEmailAddress);
                    MailMessage mailMessage = new MailMessage();
                    MailAddress fromMail = new MailAddress(fromEmailAddress);
                    // Sender e-mail address.
                    mailMessage.From = fromMail;
                    // Recipient e-mail address.
                    mailMessage.To.Add(new MailAddress(toEmailAddress));
                    // Subject of e-mail
                    mailMessage.Subject = subject;
                    mailMessage.Body = customisedMail.ToString();
                    mailMessage.IsBodyHtml = true;
                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Send(mailMessage);
                    reader.Dispose();
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }
        /// <summary>
        /// Send epub conversion mail.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="bookName"></param>
        /// <param name="userName"></param>
        /// <param name="toEmailAddress"></param>
        /// <param name="fromEmailAddress"></param>
        public static void SendEpubConversionMail(string subject, string bookName, string userName, string toEmailAddress, string fromEmailAddress)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(string.Concat(AppDomain.CurrentDomain.BaseDirectory, "HtmlMails\\ConversionStatus.html"));
                if (reader != null)
                {
                    string readFile = reader.ReadToEnd();
                    string customisedMail = string.Empty;
                    customisedMail = readFile;
                    customisedMail = customisedMail.Replace("$$UserName$$", string.IsNullOrEmpty(userName) ? "" : userName);
                    customisedMail = customisedMail.Replace("$$BookName$$", string.IsNullOrEmpty(bookName) ? "" : bookName);
                    MailMessage mailMessage = new MailMessage();
                    MailAddress fromMail = new MailAddress(fromEmailAddress);
                    // Sender e-mail address.
                    mailMessage.From = fromMail;
                    // Recipient e-mail address.
                    mailMessage.To.Add(new MailAddress(toEmailAddress));
                    // Subject of e-mail
                    mailMessage.Subject = subject;
                    mailMessage.Body = customisedMail.ToString();
                    mailMessage.IsBodyHtml = true;
                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Send(mailMessage);
                    reader.Dispose();
                }
            }
            catch (System.Exception ex)
            {
              //  Logger.Write(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// Send epub conversion mail.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="bookName"></param>
        /// <param name="userName"></param>
        /// <param name="toEmailAddress"></param>
        /// <param name="fromEmailAddress"></param>
        public static bool SendPasswordResetMail(string subject, string pwdResetLink, string userName, string toEmailAddress, string fromEmailAddress)
        {
            bool isMailSent = false;
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(string.Concat(AppDomain.CurrentDomain.BaseDirectory, "HtmlMails\\ForgotPassword.html"));
                if (reader != null)
                {
                    string readFile = reader.ReadToEnd();
                    string customisedMail = string.Empty;
                    customisedMail = readFile;
                    customisedMail = customisedMail.Replace("$$UserName$$", string.IsNullOrEmpty(userName) ? "" : userName);
                    customisedMail = customisedMail.Replace("$$PwdResetLink$$", string.IsNullOrEmpty(pwdResetLink) ? "" : pwdResetLink);
                    MailMessage mailMessage = new MailMessage();
                    MailAddress fromMail = new MailAddress(fromEmailAddress);
                    // Sender e-mail address.
                    mailMessage.From = fromMail;
                    // Recipient e-mail address.
                    mailMessage.To.Add(new MailAddress(toEmailAddress));
                    // Subject of e-mail
                    mailMessage.Subject = subject;
                    mailMessage.Body = customisedMail.ToString();
                    mailMessage.IsBodyHtml = true;
                    SmtpClient smtpClient = new SmtpClient();
                    smtpClient.Send(mailMessage);
                    reader.Dispose();
                    isMailSent= true;
                }
            }
            catch (System.Exception ex)
            {
                //Logger.Write(ex);
                isMailSent= false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            return isMailSent;
        }
        /// <summary>
        /// Encrypt a string using dual encryption method. Return a encrypted cipher Text
        /// </summary>
        /// <param name="toEncrypt">string to be encrypted</param>
        /// <param name="useHashing">use hashing? send to for extra secirity</param>
        /// <returns></returns>
        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            // Get the key from config file
            string key = "kj@18231";
            //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));
            //System.Windows.Forms.MessageBox.Show(key);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        /// <summary>
        /// DeCrypt a string using dual encryption method. Return a DeCrypted clear string
        /// </summary>
        /// <param name="cipherString">encrypted string</param>
        /// <param name="useHashing">Did you use hashing to encrypt this data? pass true is yes</param>
        /// <returns></returns>
        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            cipherString = cipherString.Replace(" ", "+");
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();
            //Get your key from config file to open the lock!
            string key = "kj@18231";
            //string key = (string)settingsReader.GetValue("SecurityKey", typeof(String));

            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static string CreateContentDirectory(string batchPath, string fileName)
        {
            var subFolder = "";
            subFolder = String.Concat(batchPath, "\\", fileName);
            if (Directory.Exists((subFolder)))
                DeleteDirectory((subFolder));
            var dir = Directory.CreateDirectory((subFolder));
            return dir.Exists ? dir.FullName : "";
        }
       

    }
    
}
