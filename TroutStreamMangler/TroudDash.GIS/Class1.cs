﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using NetTopologySuite.LinearReferencing;

namespace TroudDash.GIS
{
    public interface ILinearReference
    {
        IEnumerable<double[]> GetIntersections(IMultiLineString linestring, IGeometry multipolygon);
        IEnumerable<double[]> GetIntersections(ILineString linestring, IGeometry multipolygon);
        IEnumerable<double> GetIntersectionOfLine(ILineString primaryLine, ILineString subline);
    }

    public class LinearReference : ILinearReference
    {
        public IEnumerable<double[]> GetIntersections(IMultiLineString multilinestring, IGeometry multipolygon)
        {
            return multilinestring.Geometries.SelectMany(i => GetIntersections(i as ILineString, multipolygon));
        }

        public IEnumerable<double[]> GetIntersections(ILineString stream, IGeometry multipolygon)
        {
            var lil = new LengthIndexedLine(stream);
            var result = stream.Intersection(multipolygon) as IMultiLineString;
            var geoms = result.Geometries;
            return geoms.Select(i => i as ILineString).Select(lil.IndicesOf);
        }

        public IEnumerable<double> GetIntersectionOfLine(ILineString primaryLine, ILineString subline)
        {
            var lil = new LengthIndexedLine(primaryLine);
            var result = lil.IndicesOf(subline);
            return result;
        }
    }
}
