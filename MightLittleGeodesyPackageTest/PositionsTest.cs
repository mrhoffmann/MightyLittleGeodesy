namespace MightyLittleGeodesyTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MightyLittleGeodesy.Classes;
    using MightyLittleGeodesy.Positions;
    using System;

    [TestClass]
    public class ConversionTests
    {
        #region Known points
        [TestMethod]
        [DataRow(59.3306, 18.0596, 1628315, 6581036)] //   Stockholm Centralstation
        [DataRow(59.3275, 18.0719, 1629027, 6580715)] //           Kungliga slottet
        [DataRow(59.3258, 18.1025, 1630775, 6580585)] //                    Skansen
        [DataRow(59.3280, 18.0919, 1630163, 6580809)] //                 Vasamuseet
        [DataRow(59.3257, 18.0719, 1629034, 6580514)] //                 Gamla Stan
        public void TestKnownPoints(double wgsLat, double wgsLon, double rt90Lat, double rt90Lon)
        {
            var gaussKreuger = new GaussKreuger();
            gaussKreuger.swedishParams("rt90_2.5_gon_v");

            var rt90Coordinates = gaussKreuger.geodetic_to_grid(wgsLat, wgsLon);
            var easting = rt90Coordinates[0];
            var northing = rt90Coordinates[1];
            var convertedWGS84 = gaussKreuger.grid_to_geodetic(easting, northing);
            var latitudeDifference = wgsLat - convertedWGS84[0];
            var longitudeDifference = wgsLon - convertedWGS84[1];
            var tolerance = 0.00018; //about 20 cm (7.87 inches)

            //check that the tolerance is within or equal to the accepted tolerance above
            Assert.IsTrue(Math.Abs(latitudeDifference) < tolerance && Math.Abs(longitudeDifference) <= tolerance);
            Assert.AreEqual(Math.Round(easting, 0), rt90Lon);
            Assert.AreEqual(Math.Round(northing, 0), rt90Lat);
            //validate the range to make sure that the conversion of rt90 was okay
            Assert.IsTrue(easting >= 6110000 && easting <= 7680000);
            Assert.IsTrue(northing >= 1200000 && northing <= 1900000);
        }

        #endregion

        #region WGS84
        [TestMethod]
        public void RT90ToWGS84()
        {
            var position = new RT90Position(6583052, 1627548);
            var wgsPos = position.ToWGS84();

            // Values from Hitta.se for the conversion
            var latFromHitta = 59.3489;
            var lonFromHitta = 18.0473;

            var lat = Math.Round(wgsPos.Latitude, 4);
            var lon = Math.Round(wgsPos.Longitude, 4);

            Assert.AreEqual(latFromHitta, lat);
            Assert.AreEqual(lonFromHitta, lon);

            // String values from Lantmateriet.se, they convert DMS only.
            // Reference: http://www.lantmateriet.se/templates/LMV_Enkelkoordinattransformation.aspx?id=11500
            var latDmsStringFromLM = "N 59º 20' 56.09287\"";
            var lonDmsStringFromLM = "E 18º 2' 50.34806\"";

            Assert.AreEqual(latDmsStringFromLM, wgsPos.LatitudeToString(WGS84Position.WGS84Format.DegreesMinutesSeconds));
            Assert.AreEqual(lonDmsStringFromLM, wgsPos.LongitudeToString(WGS84Position.WGS84Format.DegreesMinutesSeconds));
        }

        [TestMethod]
        public void WGS84ParseString()
        {
            // Values from Eniro.se
            var wgsPosDM = new WGS84Position("N 62º 10.560' E 015º 54.180'", WGS84Position.WGS84Format.DegreesMinutes);
            var wgsPosDMs = new WGS84Position("N 62º 10' 33.60\" E 015º 54' 10.80\"", WGS84Position.WGS84Format.DegreesMinutesSeconds);

            Assert.AreEqual(62.176, Math.Round(wgsPosDM.Latitude, 3));
            Assert.AreEqual(15.903, Math.Round(wgsPosDM.Longitude, 3));

            Assert.AreEqual(62.176, Math.Round(wgsPosDMs.Latitude, 3));
            Assert.AreEqual(15.903, Math.Round(wgsPosDMs.Longitude, 3));
        }
        #endregion

        #region RT90
        [TestMethod]
        public void WGS84ToRT90()
        {
            var wgsPos = new WGS84Position("N 59º 58' 55.23\" E 017º 50' 06.12\"", WGS84Position.WGS84Format.DegreesMinutesSeconds);
            var rtPos = new RT90Position(wgsPos);

            // Conversion values from Lantmateriet.se, they convert from DMS only.
            // Reference: http://www.lantmateriet.se/templates/LMV_Enkelkoordinattransformation.aspx?id=11500
            var xPosFromLM = 6653174.343;
            var yPosFromLM = 1613318.742;

            Assert.AreEqual(Math.Round(rtPos.Latitude, 3), xPosFromLM);
            Assert.AreEqual(Math.Round(rtPos.Longitude, 3), yPosFromLM);
        }

        [TestMethod]
        [DataRow(59.3293, 18.0686, 59.32929999816457, 18.068600003085077)]
        public void ToAndFromRT90(double lat, double lon, double expectedLat, double expectedLon)
        {
            var wgsPos = new WGS84Position(lat, lon);
            var wgsBack = new RT90Position(wgsPos).ToWGS84();

            Assert.AreEqual(wgsBack.Longitude, expectedLon);
            Assert.AreEqual(wgsBack.Latitude, expectedLat);
            Assert.AreEqual(Math.Round(wgsBack.Latitude, 4), lat);
            Assert.AreEqual(Math.Round(wgsBack.Longitude, 4), lon);
        }

        [TestMethod]
        [DataRow(59.329300, 18.068600, 6580909, 1628833)]
        public void ToRT90(double lat, double lon, double expectedLat, double expectedLon)
        {
            var wgsPos = new WGS84Position(lat, lon);
            var wgsBack = new RT90Position(wgsPos);

            Assert.AreNotEqual(wgsBack.Longitude, lon);
            Assert.AreNotEqual(wgsBack.Latitude, lat);
            Assert.AreEqual(Math.Round(wgsBack.Longitude, MidpointRounding.ToEven), expectedLon);
            Assert.AreEqual(Math.Round(wgsBack.Latitude, MidpointRounding.ToEven), expectedLat);
        }
        #endregion

        #region SWEREF99 TM
        [TestMethod]
        public void WGS84ToSweref()
        {
            var wgsPos = new WGS84Position();

            wgsPos.SetLatitudeFromString("N 59º 58' 55.23\"", WGS84Position.WGS84Format.DegreesMinutesSeconds);
            wgsPos.SetLongitudeFromString("E 017º 50' 06.12\"", WGS84Position.WGS84Format.DegreesMinutesSeconds);

            var rtPos = new SWEREF99Position(wgsPos, SWEREFProjection.sweref_99_tm);

            // Conversion values from Lantmateriet.se, they convert from DMS only.
            // Reference: http://www.lantmateriet.se/templates/LMV_Enkelkoordinattransformation.aspx?id=11500
            var xPosFromLM = 6652797.165;
            var yPosFromLM = 658185.201;

            Assert.AreEqual(Math.Round(rtPos.Latitude, 3), xPosFromLM);
            Assert.AreEqual(Math.Round(rtPos.Longitude, 3), yPosFromLM);
        }

        [TestMethod]
        public void SwerefToWGS84()
        {
            var swePos = new SWEREF99Position(6652797.165, 658185.201);
            var wgsPos = swePos.ToWGS84();

            // String values from Lantmateriet.se, they convert DMS only.
            // Reference: http://www.lantmateriet.se/templates/LMV_Enkelkoordinattransformation.aspx?id=11500
            var latDmsStringFromLM = "N 59º 58' 55.23001\"";
            var lonDmsStringFromLM = "E 17º 50' 6.11997\"";

            Assert.AreEqual(latDmsStringFromLM, wgsPos.LatitudeToString(WGS84Position.WGS84Format.DegreesMinutesSeconds));
            Assert.AreEqual(lonDmsStringFromLM, wgsPos.LongitudeToString(WGS84Position.WGS84Format.DegreesMinutesSeconds));
        }
        #endregion
    }
}