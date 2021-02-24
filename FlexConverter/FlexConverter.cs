using System;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Drawing;

namespace FlexConverter
{
    public partial class FlexConverter : Form
    {
        String alpha = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        String alpha2 = "abcdefghijklmnopqrstuvwxyz";


        public FlexConverter()
        {
            InitializeComponent();
            cb_xorType.SelectedIndex = 0;
            cb_xorOutput.SelectedIndex = 0;
            cb_ascii.SelectedIndex = 0;
            cb_hex.SelectedIndex = 0;

            cb_unitesDistance.Items.AddRange(new String[] {
                "km -> mi", "mi -> km",
                "m -> ft", "ft -> m",
                "cm -> in", "in -> cm",
                "in -> ft", "ft -> in",
                "m -> yd", "yd -> m"
            });

            cb_unitesTemp.Items.AddRange(new String[] { "°C -> °F", "°F -> °C" });

            cb_unitesPoids.Items.AddRange(new String[] {
                "kg -> lb", "lb -> kg",
                "g -> oz", "oz -> g",
                "lb -> oz", "oz -> lb"
            });


            cb_unitesAngle.Items.AddRange(new String[] {
                "deg -> mil", "mil -> deg",
                "deg -> grad", "grad -> deg",
                "mil -> grad", "grad -> mil"
            });

        }

        #region nombres


        private void ClearNombres()
        {
            tb_nbBin.Clear();
            tb_nbDec.Clear();
            tb_nbHex.Clear();
            tb_nbOct.Clear();

        }

        private String FromStringToBaseX(String str, int toBase)
        {
            String finalstr = "";

            for (int i = 0, n = str.Length; i < n; i++)
            {
                finalstr += FromIntToBaseX((int)str[i], toBase) + " ";
            }

            //return finalstr.TrimEnd();
            return finalstr;
        }

        private String FromIntToBaseX(int x, int toBase)
        {
            String tmpstr = "";

            while (x > 0)
            {
                int idx = (int)(x % toBase);
                tmpstr = alpha[idx] + tmpstr;
                x /= toBase;
            }

            if (toBase == 16) tmpstr = tmpstr.PadLeft(2, '0');

            return tmpstr;
        }

        private int FromAnyToBase10(String input, int fromBase)
        {
            int result = 0;
            input = input.ToUpper();

            for (int i = input.Length - 1, power = 0; i >= 0; i--, power++)
            {
                int idx = alpha.IndexOf(input[i]);
                result += (idx * (int)Math.Pow(fromBase, power));
            }
            return result;
        }


        private void b_nbFromDec_Click(object sender, EventArgs e)
        {
            String str = tb_nbDec.Text;
            ClearNombres();
            tb_nbDec.Text = str;

            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tab.Length; i++)
            {
                int x;

                if (int.TryParse(tab[i], out x))
                {
                    tb_nbBin.Text += FromIntToBaseX(x, 2) + " ";
                    tb_nbOct.Text += FromIntToBaseX(x, 8) + " ";
                    tb_nbHex.Text += FromIntToBaseX(x, 16) + " ";
                }
            }
        }

        private void b_nbFromHex_Click(object sender, EventArgs e)
        {
            String str = tb_nbHex.Text;
            ClearNombres();
            tb_nbHex.Text = str;

            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tab.Length; i++)
            {
                int x = FromAnyToBase10(tab[i], 16);
                tb_nbBin.Text += FromIntToBaseX(x, 2) + " ";
                tb_nbOct.Text += FromIntToBaseX(x, 8) + " ";
                tb_nbDec.Text += FromIntToBaseX(x, 10) + " ";
            }
        }

        private void b_nbFromOct_Click(object sender, EventArgs e)
        {
            String str = tb_nbOct.Text;
            ClearNombres();
            tb_nbOct.Text = str;

            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tab.Length; i++)
            {
                int x = FromAnyToBase10(tab[i], 8);

                tb_nbBin.Text += FromIntToBaseX(x, 2) + " ";
                tb_nbDec.Text += FromIntToBaseX(x, 10) + " ";
                tb_nbHex.Text += FromIntToBaseX(x, 16) + " ";
            }
        }

        private void b_nbFromBin_Click(object sender, EventArgs e)
        {
            String str = tb_nbBin.Text;
            ClearNombres();
            tb_nbBin.Text = str;

            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tab.Length; i++)
            {
                int x = FromAnyToBase10(tab[i], 2);

                tb_nbDec.Text += FromIntToBaseX(x, 10) + " ";
                tb_nbOct.Text += FromIntToBaseX(x, 8) + " ";
                tb_nbHex.Text += FromIntToBaseX(x, 16) + " ";
            }
        }

        #endregion


        #region chaines

        private void ClearChaines()
        {
            tb_strClair.Clear();
            tb_strHex.Clear();
            tb_strBin.Clear();
            tb_strAscii.Clear();
        }

        private String formatHex(String str)
        {
            str = FromStringToBaseX(str, 16);
            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            str = "";
            for (int i = 0; i < tab.Length; i++)
            {
                if (cb_hex.SelectedIndex == 1)
                    str += "\\x" + tab[i];
                else if (cb_hex.SelectedIndex == 2)
                    str += "%" + tab[i];
                else
                    str += tab[i] + " ";
            }

            return str;
        }

        private String formatASCII(String str)
        {
            str = FromStringToBaseX(str, 10);
            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            str = "";
            for (int i = 0; i < tab.Length; i++)
            {
                if (cb_ascii.SelectedIndex == 1)
                    str += "&#" + tab[i] + ";";
                else if (cb_ascii.SelectedIndex == 2)
                    str += "%" + tab[i];
                else
                    str += tab[i] + " ";
            }

            return str;
        }

        private String formatBin(String str)
        {
            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            str = "";
            for (int i = 0; i < tab.Length; i++)
            {
                str += FromStringToBaseX(str, 2).PadLeft(8, '0') + " ";
            }
            return str;
        }


        private void b_strFromClair_Click(object sender, EventArgs e)
        {
            String str = tb_strClair.Text;
            ClearChaines();
            tb_strClair.Text = str;

            tb_strHex.Text = formatHex(str);
            tb_strAscii.Text = formatASCII(str);
            tb_strBin.Text = FromStringToBaseX(str, 2);
        }


        private void b_strFromHex_Click(object sender, EventArgs e)
        {
            String str = tb_strHex.Text;
            ClearChaines();
            tb_strHex.Text = str;

            if (cb_hex.SelectedIndex == 1) str = str.Replace(@"\x", " ");
            else if (cb_hex.SelectedIndex == 2) str = str.Replace("%", " ");


            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tab.Length; i++)
            {
                int x = FromAnyToBase10(tab[i], 16);
                tb_strClair.Text += (char)x;
                tb_strAscii.Text += x + " ";
                tb_strBin.Text += FromIntToBaseX(x, 2) + " ";
            }
        }

        private void b_strFromASCII_Click(object sender, EventArgs e)
        {
            String str = tb_strAscii.Text;
            ClearChaines();
            tb_strAscii.Text = str;

            if (cb_ascii.SelectedIndex == 1) str = str.Replace("&#", " ").Replace(";", "");
            else if (cb_ascii.SelectedIndex == 2) str = str.Replace("%", " ");

            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tab.Length; i++)
            {
                int x;
                if (int.TryParse(tab[i], out x))
                {
                    tb_strClair.Text += (char)x;
                    tb_strHex.Text += FromIntToBaseX(x, 16) + " ";
                    tb_strBin.Text += FromIntToBaseX(x, 2) + " ";
                }

            }
        }

        private void b_strFromBin_Click(object sender, EventArgs e)
        {
            String str = tb_strBin.Text;
            ClearChaines();
            tb_strBin.Text = str;

            String[] tab = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < tab.Length; i++)
            {
                int x = FromAnyToBase10(tab[i], 2);
                tb_strAscii.Text += x + " ";
                tb_strClair.Text += (char)x;
                tb_strHex.Text += FromIntToBaseX(x, 16) + " ";

            }
        }

        #endregion


        #region hash

        private void b_hashFromClair_Click(object sender, EventArgs e)
        {
            tb_hashMd5.Text = md5(tb_hashClair.Text);
            tb_hashSha1.Text = sha1(tb_hashClair.Text);
            tb_hashSha256.Text = sha256(tb_hashClair.Text);
            tb_hashMd5unix.Text = Unix_MD5Crypt.MD5Crypt.crypt(tb_hashClair.Text, tb_SaltMd5unix.Text);
        }

        private String md5(String str)
        {
            MD5 ha = MD5.Create();
            return BitConverter.ToString(ha.ComputeHash(UTF8Encoding.UTF8.GetBytes(str))).Replace("-", "");
        }

        private String sha1(String str)
        {
            SHA1 ha = SHA1.Create();
            return BitConverter.ToString(ha.ComputeHash(UTF8Encoding.UTF8.GetBytes(str))).Replace("-", "");
        }

        private String sha256(String str)
        {
            SHA256 ha = SHA256.Create();
            return BitConverter.ToString(ha.ComputeHash(UTF8Encoding.UTF8.GetBytes(str))).Replace("-", "");
        }

        private String des(String str)
        {
            return str;
        }

        #endregion


        #region XOR
        private int XorInt(int x, int y)
        {
            return x ^ y;
        }

        private String XorString(String str, String key, String output)
        {
            String result = "";
            int idxKey = 0;

            for (int i = 0; i < str.Length; i++)
            {
                int x = (int)str[i];
                int y = (int)key[idxKey];
                int r = XorInt(x, y);

                if (output == "String") result += (char)r;
                else if (output == "Int") result += r + " ";
                else if (output == "Hex") result += FromIntToBaseX(r, 16) + " ";
                else if (output == "Bin") result += FromIntToBaseX(r, 2) + " ";

                if (++idxKey == key.Length) idxKey = 0;
            }

            return result;
        }


        private String Xor(String text, String key, String type, String output)
        {
            String[] tabText = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            String[] tabKey = key.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tabText.Length == 0) return "";
            if (tabKey.Length == 0) return "";

            int idxKey = 0;
            String result = "";

            for (int i = 0; i < tabText.Length; i++)
            {
                int x = 0, y = 0, r;
                if (type == "Int") //Integer
                {
                    if (!int.TryParse(tabText[i], out x)) x = 0;
                    if (!int.TryParse(tabKey[idxKey], out y)) x = 0;
                }
                else if (type == "Hex") //Hex
                {
                    x = FromAnyToBase10(tabText[i], 16);
                    y = FromAnyToBase10(tabKey[idxKey], 16);
                }
                else if (type == "Bin") //Bin
                {
                    x = FromAnyToBase10(tabText[i], 2);
                    y = FromAnyToBase10(tabKey[idxKey], 2);
                }

                r = XorInt(x, y);
                if (output == "String") result += (char)r;
                else if (output == "Int") result += r + " ";
                else if (output == "Hex") result += FromIntToBaseX(r, 16) + " ";
                else if (output == "Bin") result += FromIntToBaseX(r, 2) + " ";

                if (++idxKey == tabKey.Length) idxKey = 0;
            }

            return result;
        }



        public String Substitution(String text, String key, bool encrypt)
        {
            String newstr = "";
            text = text.ToLower();
            key = key.ToLower();
            int idxKey = 0;

            for (int i = 0; i < text.Length; i++)
            {
                int idx = alpha2.IndexOf(text[i]);

                if (idx > -1)
                {
                    int idx2 = alpha2.IndexOf(key[idxKey]);
                    if (encrypt) newstr += alpha2[(idx + idxKey) % alpha2.Length];
                    else newstr += alpha2[(alpha2.Length + (idx - idxKey)) % alpha2.Length];

                    if (++idxKey == key.Length) idxKey = 0;
                }
                else
                    newstr += text[i];
            }

            return newstr;
        }

        private void b_xor_Click(object sender, EventArgs e)
        {
            String type = cb_xorType.SelectedItem.ToString();
            String output = cb_xorOutput.SelectedItem.ToString();

            if (type == "String") tb_xorResult.Text = XorString(tb_xor_clair.Text, tb_xorKey.Text, output);
            else tb_xorResult.Text = Xor(tb_xor_clair.Text, tb_xorKey.Text, type, output);
            
            tb_substitution.Text = Substitution(tb_xor_clair.Text, tb_xorKey.Text, true);
            tb_substitution2.Text = Substitution(tb_xor_clair.Text, tb_xorKey.Text, false);
        }


        #endregion


        #region encodeurs

        private void ClearEncodeurs()
        {
            tb_encClair.Clear();
            tb_encBackwards.Clear();
            tb_encRot13.Clear();
            tb_encBase64.Clear();
            tb_encUrl.Clear();
        }

        private void b_encFromString_Click(object sender, EventArgs e)
        {
            String str = tb_encClair.Text;
            ClearEncodeurs();
            tb_encClair.Text = str;

            tb_encBackwards.Text = Backwards(str);
            tb_encRot13.Text = Rot13(str);
            tb_encBase64.Text = Base64Encode(str);
            tb_encUrl.Text = UrlEncode(str);

        }

        private void b_encFromBase64_Click(object sender, EventArgs e)
        {
            String str = tb_encBase64.Text;
            ClearEncodeurs();
            tb_encBase64.Text = str;

            tb_encClair.Text = Base64Decode(str);
        }

        private void b_encFromUrl_Click(object sender, EventArgs e)
        {
            String str = tb_encUrl.Text;
            ClearEncodeurs();
            tb_encUrl.Text = str;

            tb_encClair.Text = UrlDecode(str);
        }


        private String Base64Encode(String str)
        {
            Byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(str);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        private String Base64Decode(String str)
        {
            Byte[] base64EncodedBytes = System.Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private String UrlEncode(String str)
        {
            return Uri.EscapeDataString(str);
        }

        private String UrlDecode(String str)
        {
            return Uri.UnescapeDataString(str);
        }



        private String Backwards(String str)
        {
            String newstr = "";

            for (int i = str.Length; i > 0;)
            {
                newstr += str[--i];
            }

            return newstr;
        }



        private String Rot13(String str)
        {
            String newstr = "";
            str = str.ToLower();

            for (int i = 0; i < str.Length; i++)
            {
                int idx = alpha2.IndexOf(str[i]);
                if (idx > -1)
                    newstr += alpha2[(idx + 13) % alpha2.Length];
                else
                    newstr += str[i];
            }

            return newstr;
        }






        #endregion


        #region monnaies
        private void b_monConvert_Click(object sender, EventArgs e)
        {
            Single from;
            Single taux;

            tb_monFrom.Text = tb_monFrom.Text.Replace('.', ',');
            tb_monTaux.Text = tb_monTaux.Text.Replace('.', ',');

            if (Single.TryParse(tb_monFrom.Text, out from))
            {
                if (Single.TryParse(tb_monTaux.Text, out taux))
                {
                    tb_monTo.Text = (from * taux).ToString();
                }
            }

        }
        #endregion


        #region unités


        private void cb_unitesDistance_SelectedIndexChanged(object sender, EventArgs e)
        {

            Double from;
            Double to;

            tb_uniFrom.Text = tb_uniFrom.Text.Replace('.', ',');

            if (Double.TryParse(tb_uniFrom.Text, out from))
            {
                if (cb_unitesDistance.SelectedIndex == 0) to = from * 0.62137;
                else if (cb_unitesDistance.SelectedIndex == 1) to = from / 0.62137;
                else if (cb_unitesDistance.SelectedIndex == 2) to = from * 3.2808;
                else if (cb_unitesDistance.SelectedIndex == 3) to = from / 3.2808;
                else if (cb_unitesDistance.SelectedIndex == 4) to = from * 0.39370;
                else if (cb_unitesDistance.SelectedIndex == 5) to = from / 0.39370;
                else if (cb_unitesDistance.SelectedIndex == 6) to = from / 12;
                else if (cb_unitesDistance.SelectedIndex == 7) to = from * 12;
                else if (cb_unitesDistance.SelectedIndex == 8) to = from * 1.0936;
                else if (cb_unitesDistance.SelectedIndex == 9) to = from / 1.0936;
                else to = 0;

                tb_uniTo.Text = Math.Round(to, 2).ToString();
            }
        }


        private void cb_unitesTemp_SelectedIndexChanged(object sender, EventArgs e)
        {
            Double from;
            Double to;

            tb_uniTempFrom.Text = tb_uniTempFrom.Text.Replace('.', ',');

            if (Double.TryParse(tb_uniTempFrom.Text, out from))
            {
                if (cb_unitesTemp.SelectedIndex == 0) to = from * 1.8 + 32;
                else if (cb_unitesTemp.SelectedIndex == 1) to = (from - 32) / 1.8;
                else to = 0;

                tb_uniTempTo.Text = Math.Round(to, 2).ToString();
            }
        }

        private void cb_unitesPoids_SelectedIndexChanged(object sender, EventArgs e)
        {
            Double from;
            Double to;

            tb_uniPoidsFrom.Text = tb_uniPoidsFrom.Text.Replace('.', ',');

            if (Double.TryParse(tb_uniPoidsFrom.Text, out from))
            {
                if (cb_unitesPoids.SelectedIndex == 0) to = from * 2.2046;
                else if (cb_unitesPoids.SelectedIndex == 1) to = from / 2.2046;
                else if (cb_unitesPoids.SelectedIndex == 2) to = from * 0.035274;
                else if (cb_unitesPoids.SelectedIndex == 3) to = from / 0.035274;
                else if (cb_unitesPoids.SelectedIndex == 4) to = from / 0.062500;
                else if (cb_unitesPoids.SelectedIndex == 5) to = from * 0.062500;
                else to = 0;

                tb_uniPoidsTo.Text = Math.Round(to, 2).ToString();
            }
        }



        private void cb_unitesAngle_SelectedIndexChanged(object sender, EventArgs e)
        {
            Double from;
            Double to;

            tb_uniAngleFrom.Text = tb_uniAngleFrom.Text.Replace('.', ',');

            if (Double.TryParse(tb_uniAngleFrom.Text, out from))
            {
                if (cb_unitesAngle.SelectedIndex == 0) to = from / 360 * 6400;
                else if (cb_unitesAngle.SelectedIndex == 1) to = from / 6400 * 360;
                else if (cb_unitesAngle.SelectedIndex == 2) to = from / 360 * 400;
                else if (cb_unitesAngle.SelectedIndex == 3) to = from / 400 * 360;
                else if (cb_unitesAngle.SelectedIndex == 4) to = from / 6400 * 400;
                else if (cb_unitesAngle.SelectedIndex == 5) to = from / 400 * 6400;
                else to = 0;

                tb_uniAngleTo.Text = Math.Round(to, 2).ToString();
            }
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            Color c = colorDialog1.Color;

            String txt = "RGB (A) : " + c.R + " " + c.G + " " + c.B + " (" + c.A + ") " + Environment.NewLine;
            txt += "couleur système : " + c.ToKnownColor() + Environment.NewLine;
            txt += "Int : " + c.ToArgb() + Environment.NewLine;
            txt += "#" + FromIntToBaseX(c.R, 16) + FromIntToBaseX(c.G, 16).ToString() + FromIntToBaseX(c.B, 16).ToString() + Environment.NewLine;

            textBox1.Text = txt;
        }




        #region coordonnées

        private void ConvertFromDec(Double y, Double x)
        {
            tb_dmsY.Text = Coordonnees.DecToDMS(y, true);
            tb_dmsX.Text = Coordonnees.DecToDMS(x, false);

            Coordonnees.Coord c = Coordonnees.Wgs84toLambert93(y, x);
            tb_lambertY.Text = c.Y.ToString();
            tb_lambertX.Text = c.X.ToString();

            c = Coordonnees.Wgs84toUTM(y, x);
            tb_utmY.Text = (c.Y / 1000).ToString();
            tb_utmX.Text = (c.X / 1000).ToString();
            tb_utmZone.Text = c.Zone.ToString();
            tb_utmFuseau.Text = c.Fuseau.ToString();

            tb_MGRS.Text = Coordonnees.UTM_MGRS(c.X, c.Y, c.Fuseau, c.Zone);

        }

        private void b_fromDecimales_Click(object sender, EventArgs e)
        {
            Double y = Coordonnees.ConvertToDouble(tb_DecY.Text);
            Double x = Coordonnees.ConvertToDouble(tb_DecX.Text);

            ConvertFromDec(y, x);
        }

        private void b_fromDMS_Click(object sender, EventArgs e)
        {
            Double y = Coordonnees.CleanCoord(tb_dmsY.Text);
            Double x = Coordonnees.CleanCoord(tb_dmsX.Text);

            tb_DecY.Text = y.ToString();
            tb_DecX.Text = x.ToString();

            ConvertFromDec(y, x);
        }

        private void b_fromLambert_Click(object sender, EventArgs e)
        {
            tb_lambertY.Text = Coordonnees.FixDecSeparator(tb_lambertY.Text);
            tb_lambertY.Text = Coordonnees.FixDecSeparator(tb_lambertY.Text);

            Double y = double.Parse(tb_lambertY.Text);
            Double x = double.Parse(tb_lambertX.Text);

            Coordonnees.Lambert93toWgs84(y, x);
        }

        private void b_fromUTM_Click(object sender, EventArgs e)
        {
            String zone = tb_utmZone.Text;
            int fuseau = int.Parse(tb_utmFuseau.Text);
            Double y = Coordonnees.ConvertToDouble(tb_utmY.Text);
            Double x = Coordonnees.ConvertToDouble(tb_utmX.Text);

            Coordonnees.Coord c = Coordonnees.UTMtoWgs84(y, x, fuseau);

            ConvertFromDec(c.Y, c.X);
        }

        private void b_fromMGRS_Click(object sender, EventArgs e)
        {
            String mgrs = tb_MGRS.Text;
            Coordonnees.Coord c = Coordonnees.MGRS_UTM(mgrs);
            c = Coordonnees.UTMtoWgs84(c.Y, c.X, c.Fuseau);

            ConvertFromDec(c.Y, c.X);
        }

        #endregion
    }
}
