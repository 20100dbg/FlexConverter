using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FlexConverter
{
    public static class Coordonnees
    {
        public class Coord
        {
            public Double Y { get; set; }
            public Double X { get; set; }
            public string Zone { get; set; }
            public int Fuseau { get; set; }
        }

        public static String DecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        
        public static String FixDecSeparator(String s)
        {
            String oldSeparator = (DecimalSeparator == ",") ? "." : ",";
            s = s.Replace(oldSeparator, DecimalSeparator);
            return s;
        }

        public static Double ConvertToDouble(String s)
        {
            Double x;
            s = s.Trim();
            s = FixDecSeparator(s);

            if (!double.TryParse(s, out x)) x = -999999;
            return x;
        }

        public static Double CleanCoord(String coord)
        {
            Double x;
            coord = coord.Trim();
            coord = FixDecSeparator(coord);

            if (coord != "")
            {
                if (double.TryParse(coord, out x)) return x;
                {
                    Regex rgx = new Regex(@"^[0-9]{1,3}\s{0,}°\s{0,}[0-9]{1,2}\s{0,}'\s{0,}[0-9\.,]{1,}\s{0,}""\s{0,}[NSEW]$");
                    Match m = rgx.Match(coord);
                    if (m.Success) return DMStoDec(coord);
                }
            }

            return -999999;
        }

        public static Double DMStoDec(String dms)
        {
            String[] tab = dms.Split(new char[] { '°' });
            int degdec = int.Parse(tab[0]);

            tab = tab[1].Split(new char[] { '\'' });
            double mindec = double.Parse(tab[0]);

            tab = tab[1].Split(new char[] { '"' });
            double secdec = double.Parse(tab[0]);

            String latlong = tab[1].Trim();
            Double tmp = degdec + (mindec / 60) + (secdec / 3600);
            tmp = Math.Round(tmp, 6);

            if (latlong == "S" || latlong == "W") tmp = tmp * -1;

            return tmp;
        }

        public static String DecToDMS(Double d, Boolean isLatitude)
        {
            String cardinal = "";
            Double tmp;
            if (isLatitude) cardinal = (d < 0) ? "S" : "N";
            else cardinal = (d < 0) ? "W" : "E";

            Double deg = (d < 0) ? Math.Ceiling(d) : Math.Floor(d);
            Double min = (d - deg) * 60;

            tmp = min;
            min = (d < 0) ? Math.Ceiling(min) : Math.Floor(min);

            Double sec = Math.Round((tmp - min) * 60, 4);

            String s = Math.Abs(deg) + "° " + Math.Abs(min) + "' " + Math.Abs(sec) + "\" " + cardinal;
            return s;
        }


        //LAMBERT

        public static Double PI = 3.14159265358979;
        public static Double a = 6378137;//            ' demi grand axe de l'ellipsoide(m)
        public static Double e = 0.08181919106;    //  ' première excentricité de l'ellipsoide
        public static Double k0 = 0.9996;

        public static Double x0 = 700000;   //         ' coordonnées à l'origine
        public static Double y0 = 6600000;    //       ' coordonnées à l'origine


        public static Coord Wgs84toLambert93(double latitude, double longitude)
        {
            Double l0, lc, phi0, phi1, phi2;
            Double phi, l, gN1, gN2;
            Double gl1, gl2, gl0, gl;
            Double n, c, ys;
            Coord Wgs84toLambert93 = new Coord();

            l0 = deg2rad(3);
            lc = deg2rad(3);
            phi0 = deg2rad(46.5);//    ' latitude d'origine en radian
            phi1 = deg2rad(44);    //  ' 1er parallele automécoïque
            phi2 = deg2rad(49);     // ' 2eme parallele automécoïque


            phi = deg2rad(latitude);
            l = deg2rad(longitude);


            //calcul des grandes normales
            gN1 = a / Math.Sqrt(1 - e * e * Math.Sin(phi1) * Math.Sin(phi1));
            gN2 = a / Math.Sqrt(1 - e * e * Math.Sin(phi2) * Math.Sin(phi2));


            //calculs des latitudes isométriques
            gl1 = Math.Log(Math.Tan(Math.PI / 4 + phi1 / 2) * Math.Pow((1 - e * Math.Sin(phi1)) / (1 + e * Math.Sin(phi1)), e / 2));
            gl2 = Math.Log(Math.Tan(Math.PI / 4 + phi2 / 2) * Math.Pow((1 - e * Math.Sin(phi2)) / (1 + e * Math.Sin(phi2)), e / 2));
            gl0 = Math.Log(Math.Tan(Math.PI / 4 + phi0 / 2) * Math.Pow((1 - e * Math.Sin(phi0)) / (1 + e * Math.Sin(phi0)), e / 2));
            gl = Math.Log(Math.Tan(Math.PI / 4 + phi / 2) * Math.Pow((1 - e * Math.Sin(phi)) / (1 + e * Math.Sin(phi)), e / 2));


            //calcul de l'exposant de la projection
            n = (Math.Log((gN2 * Math.Cos(phi2)) / (gN1 * Math.Cos(phi1)))) / (gl1 - gl2);


            //calcul de la constante de projection
            c = ((gN1 * Math.Cos(phi1)) / n) * Math.Exp(n * gl1);


            //calcul des coordonnées
            ys = y0 + c * Math.Exp(-1 * n * gl0);
            Wgs84toLambert93.X = x0 + c * Math.Exp(-1 * n * gl) * Math.Sin(n * (l - lc));
            Wgs84toLambert93.Y = ys - c * Math.Exp(-1 * n * gl) * Math.Cos(n * (l - lc));

            return Wgs84toLambert93;
        }

        public static Coord Lambert93toWgs84(double Y, double X)
        {
            Double b8, b10, b16;
            Double latiso, sinphiit;
            Coord Lambert93toWgs84 = new Coord();

            b8 = 1 / 298.257222101;
            b10 = Math.Sqrt(2 * b8 - b8 * b8);
            b16 = 0.725607765053267;

            X = X - x0;
            Y = Y - 12655612.0499;

            latiso = Math.Log(11754255.426096 / Math.Sqrt((X * X) + (Y * Y))) / b16;
            sinphiit = Math.Tanh(latiso + b10 * ATanH(b10 * Math.Sin(1)));

            for (int i = 0; i < 5; i++)
            {
                sinphiit = Math.Tanh(latiso + b10 * ATanH(b10 * sinphiit));
            }


            Lambert93toWgs84.Y = Math.Asin(sinphiit) / PI * 180;
            //Lambert93toWgs84.X = ((Atn(-X / Y)) / b16 + 3 / 180 * PI) / PI * 180;
            Lambert93toWgs84.X = ((Math.Atan(-X / Y)) / b16 + 3 / 180 * PI) / PI * 180;
            return Lambert93toWgs84;
        }


        /// /// MGRS


        public static String UTM_MGRS(double UtmX, double UtmY, int UtmFuseau, String UtmZone, int Prec = -1, String CarSep = "")
        {
            String Sep, res;
            double L1, L2, Offset;
            double X, Y, Fact;

            if (Prec == -1) Prec = 5;
            else
            {
                if (Prec > 5) Prec = 5;
                if (Prec < 1) Prec = 1;
            }
            double tmp = Math.Pow(10, 5 - Prec);
            Fact = tmp * 0.5;
            if (CarSep == "") Sep = " ";
            else Sep = CarSep.Substring(0, 1);

            res = UtmFuseau.ToString().PadLeft(2,'0') + Sep + UtmZone + Sep;
            //res = Format(UtmFuseau, "00") & Sep & UtmZone & Sep
            
            L1 = ((UtmFuseau - 1) % 3) * 8 + Math.Floor(UtmX / 100000);

            if (UtmFuseau % 2 == 0) Offset = 5;
            else Offset = 0;

            L2 = ((Offset + Math.Floor(UtmY / 100000)) % 20) + 1;

            res = res + "ABCDEFGHJKLMNPQRSTUVWXYZ".Substring((int)L1-1, 1) + "ABCDEFGHJKLMNPQRSTUV".Substring((int)L2 - 1, 1) + Sep;
            X = UtmX - Math.Floor(UtmX / 100000) * 100000;
            X = X + Fact;
            Y = UtmY - Math.Floor(UtmY / 100000) * 100000;
            Y = Y + Fact;
            return res + X.ToString().PadLeft(5, '0').Substring(0, Prec) + Sep + Y.ToString().PadLeft(5, '0').Substring(0, Prec);
        }

        public static Coord MGRS_UTM(String mgrs)
        {
            Coord c = new Coord();
            c.Fuseau = MGRS_FusUTM(mgrs);
            c.Zone = MGRS_ZonUTM(mgrs);
            c.Y = MGRS_UTMNorthing(mgrs) / 1000;
            c.X = MGRS_UTMEasting(mgrs) / 1000;

            return c;
        }


        static int MGRS_FusUTM(String mgrs)
        {
            String VarIn = "";
            char c;

            for (int i = 0; i < mgrs.Length; i++)
            {
                c = mgrs.Substring(i, 1)[0];
                if (c >= (int)'A' && c <= (int)'Z') VarIn = VarIn + c;
                if (c >= (int)'0' && c <= (int)'9') VarIn = VarIn + c;
            }

            return int.Parse(VarIn.Substring(0, 2));
        }


        static string MGRS_ZonUTM(String mgrs)
        {
            String VarIn = "";
            char c;

            for (int i = 0; i < mgrs.Length; i++)
            {
                c = mgrs.Substring(i, 1)[0];
                if (c >= (int)'A' && c <= (int)'Z') VarIn = VarIn + c;
                if (c >= (int)'0' && c <= (int)'9') VarIn = VarIn + c;
            }

            return VarIn.Substring(2, 1);
        }


        static Double MGRS_UTMEasting(String mgrs)
        {
            String L1, XY;
            Double X;
            String VarIn = "";
            char c;

            for (int i = 0; i < mgrs.Length; i++)
            {
                c = mgrs.Substring(i, 1)[0];
                if (c >= (int)'A' && c <= (int)'Z') VarIn = VarIn + c;
                if (c >= (int)'0' && c <= (int)'9') VarIn = VarIn + c;
            }


            L1 = VarIn.Substring(3, 1);
            XY = VarIn.Substring(5);
            int l = XY.Length / 2;
            X = int.Parse(XY.Substring(0, l));
            int OffX = "ABCDEFGHJKLMNPQRSTUVWXYZ".IndexOf(L1); //retirer - 1
            OffX = (OffX % 8) + 1;
            OffX = OffX * 100000;
            return OffX + X;
        }


        static double MGRS_UTMNorthing(String mgrs)
        {
            int l, j, RUtm;
            String VarIn, L2, XY;
            Double Y, FloorUtm;
            VarIn = "";
            char c;
            char ZonUtm;
            Double OffY;

            for (int i = 0; i < mgrs.Length; i++)
            {
                c = mgrs.Substring(i, 1)[0];
                if (c >= (int)'A' && c <= (int)'Z') VarIn = VarIn + c;
                if (c >= (int)'0' && c <= (int)'9') VarIn = VarIn + c;
            }

            L2 = VarIn.Substring(4, 1);
            ZonUtm = VarIn.Substring(2, 1)[0];
            XY = VarIn.Substring(5);
            l = XY.Length / 2;
            Y = int.Parse(XY.Substring(XY.Length - l));
            
            OffY = "ABCDEFGHJKLMNPQRSTUV".IndexOf(L2); //retirer - 1
            int k = int.Parse(VarIn.Substring(0,2));
            j = (k / 2);
            if (k == 2 * j)
                OffY = OffY - 5;

            OffY = OffY * 100000;
            OffY = OffY + Y;

            if (ZonUtm >= (int)'N')
            {
                RUtm = "ABCDEFGHJKLMNPQRSTUVWXYZ".IndexOf(ZonUtm) - 13 + 1; //ajouter + 1
                if (RUtm > 10) RUtm = 10;
                FloorUtm = 110500 * 8 * RUtm;

                while (OffY < FloorUtm) OffY = OffY + 2000000;
            }
            else
            {
                RUtm = 13 - "ABCDEFGHJKLMNPQRSTUVWXYZ".IndexOf(ZonUtm) + 1;//ajouter + 1
                FloorUtm = 110500 * (90 - (RUtm * 8));

                while (OffY < FloorUtm) OffY = OffY + 2000000;
            }

            return OffY;
        }


        public static String Prettify(String mgrs)
        {
            return mgrs.Substring(0, 2) + " " + mgrs.Substring(3, 3) + mgrs.Substring(6, 5) + mgrs.Substring(mgrs.Length - 5);
        }


        //////////////////////////////UTM

        public static Coord Wgs84toUTM(Double LATITUDE, Double LONGITUDE)
        {
            Coord Wgs84toUTM = new Coord();

            Double lat, lng, lng0, T, c, vphi, Ac;
            double Zn;
            Double[] S = new double[5];
            Double[] lb = new double[4];

            lat = deg2rad(LATITUDE);
            lng = deg2rad(LONGITUDE);

            Double x1 = Math.Floor((LONGITUDE + 180) / 360);
            Double x2 = Math.Floor((LONGITUDE - x1 * 360 + 180) / 6);
            Zn = x2 + 1;
            
            //Zn = Int((LONGITUDE - Int((LONGITUDE + 180) / 360) * 360 + 180) / 6) + 1;

            lng0 = deg2rad(((Zn - 1) * 6) - 180 + 3);
            vphi = 1 / Math.Sqrt(1 - (Math.Pow(e, 2) * Math.Sin(lat) * Math.Sin(lat)));
            Ac = (lng - lng0) * Math.Cos(lat);


            S[1] = (1 - (Math.Pow(e, 2) / 4) - (3 * Math.Pow(e, 4) / 64) - (5 * Math.Pow(e, 6) / 256)) * lat;
            S[2] = ((3 * Math.Pow(e, 2) / 8) + (3 * Math.Pow(e, 4) / 32) + (45 * Math.Pow(e, 6) / 1024)) * Math.Sin(2 * lat);
            S[3] = ((15 * Math.Pow(e, 4) / 256) + (45 * Math.Pow(e, 6) / 1024)) * Math.Sin(4 * lat);
            S[4] = (35 * Math.Pow(e, 6) / 3072) * Math.Sin(6 * lat);
            S[0] = S[1] - S[2] + S[3] - S[4];
            T = Math.Pow(Math.Tan(lat), 2);
            c = (Math.Pow(e, 2) / (1 - Math.Pow(e, 2))) * Math.Pow(Math.Cos(lat), 2);


            lb[1] = Math.Pow(Ac, 2) / 2;
            lb[2] = (5 - T + (9 * c) + (4 * Math.Pow(c, 2))) * (Math.Pow(Ac, 4)) / 24;
            lb[3] = (61 - (58 * T) + (T * T)) * (Math.Pow(Ac, 6)) / 720;
            lb[0] = k0 * a * (S[0] + (vphi * Math.Tan(lat) * (lb[1] + lb[2] + lb[3])));


            Wgs84toUTM.Y = lb[0] / 1000 + ((lat < 0) ?  10000: 0);
            Wgs84toUTM.X = 500 + (k0 * a / 1000 * vphi * (Ac + (1 - T + c) * Math.Pow(Ac, 3) / 6 + 
                             (5 - (18 * T) + Math.Pow(T, 2)) * Math.Pow(Ac, 5) / 120));


            Wgs84toUTM.Y = Wgs84toUTM.Y * 1000;
            Wgs84toUTM.X = Wgs84toUTM.X * 1000;

            Wgs84toUTM.Zone = Utm_Letter(LATITUDE);
            Wgs84toUTM.Fuseau = (int)Zn;
            return Wgs84toUTM;
        }


        public static Coord UTMtoWgs84(Double LATITUDE, double LONGITUDE, int Fuseau, String Ltr = "")
        {
            Coord UTMtoWgs84 = new Coord();


            Double X, Y, ecctrty, eccPS;
            Double mu, e1, phi1rad;
            Double N1, T1, C1, R1, D;
            Double lng, lat;


            X = (LONGITUDE * 1000) - 500000;
            Y = (LATITUDE * 1000);
            if (Ltr != "") Y = Y - ((((int)Ltr[0]) < 78) ? 10000000 : 0);

            ecctrty = Math.Pow(e, 2);
            eccPS = ecctrty / (1 - ecctrty);

            mu = Y / k0 / (a * (1 - ecctrty / 4 - 3 * Math.Pow(ecctrty, 2) / 64 - 5 * Math.Pow(ecctrty, 3) / 256));
            e1 = (1 - Math.Sqrt(1 - ecctrty)) / (1 + Math.Sqrt(1 - ecctrty));
            phi1rad = mu + (3 * e1 / 2 - 27 * Math.Pow(e1, 3) / 32) * Math.Sin(2 * mu) + (21 * Math.Pow(e1, 2)
                      / 16 - 55 * Math.Pow(e1, 4) / 32) * Math.Sin(4 * mu) + (151 * Math.Pow(e1, 3) / 96) * Math.Sin(6 * mu);
            N1 = a / Math.Sqrt(1 - ecctrty * Math.Sin(phi1rad) * Math.Sin(phi1rad));
            T1 = Math.Pow(Math.Tan(phi1rad), 2);
            C1 = ecctrty * Math.Pow(Math.Cos(phi1rad), 2);
            R1 = a * (1 - ecctrty) / (Math.Pow(1 - ecctrty * Math.Pow(Math.Sin(phi1rad), 2), 1.5));
            D = X / (N1 * k0);

            lat = phi1rad - (N1 * Math.Tan(phi1rad) / R1) * (Math.Pow(D, 2) / 2 - (5 + 3 * T1 + 10 * C1 - 4
                 * Math.Pow(C1, 2) - 9 * eccPS) * Math.Pow(D, 4) / 24 + (61 + 90 * T1 + 298 * C1 + 45
                 * Math.Pow(T1, 2) - 252 * eccPS - 3 * Math.Pow(C1, 2)) * Math.Pow(D, 6) / 720);

            lng = (D - (1 + 2 * T1 + C1) * Math.Pow(D, 3) / 6 + (5 - 2 * C1 + 28 * T1 - 3
                  * Math.Pow(C1, 2) + 8 * eccPS + 24 * Math.Pow(T1, 2)) * Math.Pow(D, 5) / 120) / Math.Cos(phi1rad);

            UTMtoWgs84.Y = rad2deg(lat);
            UTMtoWgs84.X = rad2deg(lng) + (Fuseau - 1) * 6 - 180 + 3;


            return UTMtoWgs84;
        }

        public static Double deg2rad(Double deg)
        {
            return deg * Math.PI / 180;
        }

        public static Double rad2deg(Double rad)
        {
            return rad / Math.PI * 180;
        }

        public static Double ATanH(Double value)
        {
            return Math.Log((1 / value + 1) / (1 / value - 1)) / 2;
        }

        public static String Utm_Letter(Double lat)
        {
            string s = "";
            if (lat >= 72 && lat <= 84) s = "X";
            else if (lat >= 64 && lat <= 72) s = "W";
            else if (lat >= 56 && lat <= 64) s = "V";
            else if (lat >= 48 && lat <= 56) s = "U";
            else if (lat >= 40 && lat <= 48) s = "T";
            else if (lat >= 32 && lat <= 40) s = "S";
            else if (lat >= 24 && lat <= 32) s = "R";
            else if (lat >= 16 && lat <= 24) s = "Q";
            else if (lat >= 8 && lat <= 16) s = "P";
            else if (lat >= 0 && lat <= 8) s = "N";
            else if (lat >= -8 && lat <= 0) s = "M";
            else if (lat >= -16 && lat <= -8) s = "L";
            else if (lat >= -24 && lat <= -16) s = "K";
            else if (lat >= -32 && lat <= -24) s = "J";
            else if (lat >= -40 && lat <= -32) s = "H";
            else if (lat >= -48 && lat <= -40) s = "G";
            else if (lat >= -56 && lat <= -48) s = "F";
            else if (lat >= -64 && lat <= -56) s = "E";
            else if (lat >= -72 && lat <= -64) s = "D";
            else if (lat >= -80 && lat <= -72) s = "C";
            else s = "";
            return s;
        }



    }
}
