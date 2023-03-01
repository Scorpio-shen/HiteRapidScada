using Opc.Ua.Security.Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KpHiteOpcUaServer.NodeSetting
{
    public class OpcUtils
    {
        public static List<string> ParseDistinguishedName(string name)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(name))
            {
                return list;
            }

            char c = ',';
            bool flag = false;
            bool flag2 = false;
            for (int num = name.Length - 1; num >= 0; num--)
            {
                char c2 = name[num];
                if (c2 == '"')
                {
                    flag2 = !flag2;
                }
                else if (!flag2 && c2 == '=')
                {
                    num--;
                    while (num >= 0 && char.IsWhiteSpace(name[num]))
                    {
                        num--;
                    }

                    while (num >= 0 && (char.IsLetterOrDigit(name[num]) || name[num] == '.'))
                    {
                        num--;
                    }

                    while (num >= 0 && char.IsWhiteSpace(name[num]))
                    {
                        num--;
                    }

                    if (num >= 0)
                    {
                        c = name[num];
                    }

                    break;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            string value = null;
            string text = null;
            flag = false;
            for (int i = 0; i < name.Length; i++)
            {
                for (; i < name.Length && char.IsWhiteSpace(name[i]); i++)
                {
                }

                if (i >= name.Length)
                {
                    break;
                }

                char c3 = name[i];
                if (flag)
                {
                    char c4 = c;
                    if (i < name.Length && name[i] == '"')
                    {
                        i++;
                        c4 = '"';
                    }

                    for (; i < name.Length; i++)
                    {
                        c3 = name[i];
                        if (c3 == c4)
                        {
                            for (; i < name.Length && name[i] != c; i++)
                            {
                            }

                            break;
                        }

                        stringBuilder.Append(c3);
                    }

                    text = stringBuilder.ToString().TrimEnd();
                    flag = false;
                    stringBuilder.Length = 0;
                    stringBuilder.Append(value);
                    stringBuilder.Append('=');
                    if (text.IndexOfAny(new char[3] { '/', ',', '=' }) != -1)
                    {
                        if (text.Length > 0 && text[0] != '"')
                        {
                            stringBuilder.Append('"');
                        }

                        stringBuilder.Append(text);
                        if (text.Length > 0 && text[text.Length - 1] != '"')
                        {
                            stringBuilder.Append('"');
                        }
                    }
                    else
                    {
                        stringBuilder.Append(text);
                    }

                    list.Add(stringBuilder.ToString());
                    stringBuilder.Length = 0;
                    continue;
                }

                for (; i < name.Length; i++)
                {
                    c3 = name[i];
                    if (c3 == '=')
                    {
                        break;
                    }

                    stringBuilder.Append(c3);
                }

                value = stringBuilder.ToString().TrimEnd().ToUpperInvariant();
                stringBuilder.Length = 0;
                flag = true;
            }

            return list;
        }

        public static bool CompareDistinguishedName(string name1, string name2)
        {
            if (string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }

            List<string> list = ParseDistinguishedName(name1);
            List<string> list2 = ParseDistinguishedName(name2);
            if (list.Count != list2.Count)
            {
                return false;
            }

            list.Sort(StringComparer.OrdinalIgnoreCase);
            list2.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Compare(list[i], list2[i], StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CompareDistinguishedName(X509Certificate2 certificate, List<string> parsedName)
        {
            if (parsedName.Count == 0)
            {
                return false;
            }

            List<string> list = ParseDistinguishedName(certificate.Subject);
            if (parsedName.Count != list.Count)
            {
                return false;
            }

            parsedName.Sort(StringComparer.OrdinalIgnoreCase);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < parsedName.Count; i++)
            {
                if (string.Compare(parsedName[i], list[i], StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasApplicationURN(X509Certificate2 certificate)
        {
            X509SubjectAltNameExtension x509SubjectAltNameExtension = null;
            X509ExtensionEnumerator enumerator = certificate.Extensions.GetEnumerator();
            while (enumerator.MoveNext())
            {
                X509Extension current = enumerator.Current;
                if (current.Oid.Value == X509SubjectAltNameExtension.SubjectAltNameOid || current.Oid.Value == X509SubjectAltNameExtension.SubjectAltName2Oid)
                {
                    x509SubjectAltNameExtension = new X509SubjectAltNameExtension(current, current.Critical);
                    break;
                }
            }

            if (x509SubjectAltNameExtension != null && x509SubjectAltNameExtension.Uris.Count > 0)
            {
                string text = "urn:";
                for (int i = 0; i < x509SubjectAltNameExtension.Uris.Count; i++)
                {
                    if (string.Compare(x509SubjectAltNameExtension.Uris[i], 0, text, 0, text.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


    }
}
