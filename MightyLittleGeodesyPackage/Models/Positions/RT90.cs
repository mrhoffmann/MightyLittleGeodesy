/*
 * MightyLittleGeodesy 
 * RT90, SWEREF99 and WGS84 coordinate transformation library
 * 
 * Read my blog @ http://blog.sallarp.com
 * 
 * 
 * Copyright (C) 2009 Björn Sållarp
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this 
 * software and associated documentation files (the "Software"), to deal in the Software 
 * without restriction, including without limitation the rights to use, copy, modify, 
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to 
 * permit persons to whom the Software is furnished to do so, subject to the following 
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or 
 * substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using MightyLittleGeodesy.Classes;

namespace MightyLittleGeodesy.Positions
{
    public class RT90Position : Position
    {
        /// <summary>
        /// Create a new position using default projection (2.5 gon v);
        /// </summary>
        /// <param name="x">X on an axis being horizontal, this representing an east-west axis. (longitude)</param>
        /// <param name="y">Y on an axis being vertical, this representing an northly-southly axis. (latitude)</param>
        public RT90Position(double x, double y)
            : base(x, y, Grid.RT90)
        {
            Projection = RT90Projection.rt90_2_5_gon_v;
        }

        /// <summary>
        /// Create a new position
        /// </summary>
        /// <param name="x">X on an axis being horizontal, this representing an east-west axis. (longitude)</param>
        /// <param name="y">Y on an axis being vertical, this representing an northly-southly axis. (latitude)</param>
        /// <param name="projection"></param>
        public RT90Position(double x, double y, RT90Projection projection)
            : base(x, y, Grid.RT90)
        {
            Projection = projection;
        }

        /// <summary>
        /// Create a RT90 position by converting a WGS84 position
        /// </summary>
        /// <param name="position">WGS84 position to convert</param>
        /// <param name="rt90projection">Projection to convert to, by default rt90_2_5_gon_v</param>
        public RT90Position(WGS84Position position, RT90Projection rt90projection = RT90Projection.rt90_2_5_gon_v)
            : base(Grid.RT90)
        {
            GaussKreuger gkProjection = new();
            gkProjection.swedishParams(GetProjectionString(rt90projection));
            var lat_lon = gkProjection.geodetic_to_grid(position.Latitude, position.Longitude);
            Latitude = lat_lon[0];
            Longitude = lat_lon[1];
            Projection = rt90projection;
        }

        /// <summary>
        /// Convert the position to WGS84 format
        /// </summary>
        /// <returns></returns>
        public WGS84Position ToWGS84()
        {
            GaussKreuger gkProjection = new();
            gkProjection.swedishParams(ProjectionString);
            var lat_lon = gkProjection.grid_to_geodetic(Latitude, Longitude);

            WGS84Position newPos = new()
            {
                Latitude = lat_lon[0],
                Longitude = lat_lon[1],
                GridFormat = Grid.WGS84
            };

            return newPos;
        }

        private static string GetProjectionString(RT90Projection projection)
        {
            string retVal = projection switch
            {
                RT90Projection.rt90_7_5_gon_v => "rt90_7.5_gon_v",
                RT90Projection.rt90_5_0_gon_v => "rt90_5.0_gon_v",
                RT90Projection.rt90_2_5_gon_v => "rt90_2.5_gon_v",
                RT90Projection.rt90_0_0_gon_v => "rt90_0.0_gon_v",
                RT90Projection.rt90_2_5_gon_o => "rt90_2.5_gon_o",
                RT90Projection.rt90_5_0_gon_o => "rt90_5.0_gon_o",
                _ => "rt90_2.5_gon_v",
            };
            return retVal;
        }

        public RT90Projection Projection { get; set; }
        public string ProjectionString
        {
            get
            {
                return GetProjectionString(Projection);
            }
        }

        public override string ToString()
        {
            return string.Format("X: {0} Y: {1} Projection: {2}", Latitude, Longitude, ProjectionString);
        }
    }
}
