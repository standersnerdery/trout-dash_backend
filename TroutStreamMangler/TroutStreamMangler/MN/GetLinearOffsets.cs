using System;
using System.Collections.Generic;
using Npgsql;
using TroutDash.DatabaseImporter.Convention;

namespace TroutStreamMangler.MN
{
    public class LinearReferenceResult
    {
        public int StreamId { get; set; }
        public string StreamNumber { get; set; }
        public string StreamName { get; set; }

        public string GeometryName { get; set; }
        public int GeometryId { get; set; }

        public string IntersectionGeometry { get; set; }
        public string IntersectionGeometryType { get; set; }

        public double StartPoint { get; set; }
        public double EndPoint { get; set; }
        public double StreamLength { get; set; }
    }

    public class GetLinearOffsets
    {
        private readonly IDatabaseConnection _dbConnection;

        public GetLinearOffsets(IDatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }


        public IEnumerable<LinearReferenceResult> ExecuteBufferedLinearReferenceResults(string streamTableName,
            string streamNameColumn,
            string streamIdColumn,
            string streamId,
            string geometryTableName,
            string geometryIdColumn,
            string geometryNameColumn)
        {
            const string linearReferenceString =
                @"SELECT intersection_data.stream_id, 
       intersection_data.stream_number, 
       intersection_data.stream_name, 
       intersection_data.body_name, 
       intersection_data.body_id, 
       intersection_data.mini_line, 
       intersection_data.geometry_type, 
       St_linelocatepoint(St_linemerge(stream_route.geom), St_startpoint(intersection_data.mini_line)) AS start_point ,
       St_linelocatepoint(St_linemerge(stream_route.geom), St_endpoint(intersection_data.mini_line))   AS end_point,
       St_length(stream_route.geom)                                                                    AS outer_length
FROM   ( 
              SELECT dumped.stream_id, 
                     dumped.stream_number, 
                     dumped.stream_name, 
                     dumped.body_name, 
                     dumped.body_id, 
                     st_linemerge((dumped.geom_dump).geom)               AS mini_line, 
                     geometrytype(st_linemerge((dumped.geom_dump).geom)) AS geometry_type 
              FROM   ( 
                            SELECT stream_id, 
                                   stream_number, 
                                   stream_name, 
                                   body_name, 
                                   body_id, 
                                   st_dump(intersection_geom) AS geom_dump 
                            FROM   ( 
                                          SELECT stream.{2}                                            AS stream_id,
                                                 stream.{3}                                        stream_number,
                                                 stream.{3}                                        stream_name,
                                                 lake.{6}                                       AS body_name,
                                                 lake.{5}                                       AS body_id,
                                                 st_intersection(stream.geom, lake.geom)               AS intersection_geom,
                                                 geometrytype(st_intersection(stream.geom, lake.geom)) AS geom_type
                                          FROM   PUBLIC.{0} stream,
                                                 ( 
                                                          SELECT   new_reg                                 AS {5},
                                                                   'Some Regulation'                       AS {6},
                                                                   st_buffer(st_multi(st_union(geom)), 10) AS geom
                                                          FROM     ( 
                                                                          SELECT regulation.{5},
                                                                                 'asdf' AS {6},
                                                                                 regulation.geom
                                                                          FROM   public.{1} regulation,
                                                                                 public.{0} route
                                                                          WHERE  st_intersects(st_envelope(route.geom), regulation.geom)
                                                                          AND    route.{2} = {4}) AS subquery
                                                          GROUP BY new_reg) lake 
                                          WHERE  stream.{2} = {4} 
                                          AND    st_intersects(lake.geom, stream.geom)) AS complex ) AS dumped ) AS intersection_data,
       PUBLIC.{0} stream_route 
WHERE  stream_route.{2} = {4}";

            var sql = string.Format(linearReferenceString, streamTableName, geometryTableName, streamIdColumn,
                streamNameColumn, streamId, geometryIdColumn, geometryNameColumn);

            foreach (var linearReferenceResult in ExecuteQuery(sql))
            {
                yield return linearReferenceResult;
            }

        }

        public IEnumerable<LinearReferenceResult> ExecuteLinearReference(string streamTableName,
            string streamNameColumn,
            string streamIdColumn,
            string streamId,
            string geometryTableName,
            string geometryIdColumn,
            string geometryNameColumn)
        {
            const string linearReferenceString =
                @"SELECT intersection_data.stream_id, 
       intersection_data.stream_number, 
       intersection_data.stream_name, 
       intersection_data.body_name, 
       intersection_data.body_id, 
       intersection_data.mini_line, 
       intersection_data.geometry_type, 
       St_linelocatepoint(St_linemerge(stream_route.geom), St_startpoint(intersection_data.mini_line)) AS start_point ,
       St_linelocatepoint(St_linemerge(stream_route.geom), St_endpoint(intersection_data.mini_line))   AS end_point,
       St_length(stream_route.geom)                                                                    AS outer_length
FROM   ( 
              SELECT dumped.stream_id, 
                     dumped.stream_number, 
                     dumped.stream_name, 
                     dumped.body_name, 
                     dumped.body_id, 
                     st_linemerge((dumped.geom_dump).geom)               AS mini_line, 
                     geometrytype(st_linemerge((dumped.geom_dump).geom)) AS geometry_type 
              FROM   ( 
                            SELECT stream_id, 
                                   stream_number, 
                                   stream_name, 
                                   body_name, 
                                   body_id, 
                                   st_dump(intersection_geom) AS geom_dump 
                            FROM   ( 
                                          SELECT stream.{2}                                            AS stream_id,
                                                 stream.{3}                                        stream_number,
                                                 stream.{3}                                        stream_name,
                                                 lake.{6}                                       AS body_name,
                                                 lake.{5}                                       AS body_id,
                                                 st_intersection(stream.geom, lake.geom)               AS intersection_geom,
                                                 geometrytype(st_intersection(stream.geom, lake.geom)) AS geom_type
                                          FROM   PUBLIC.{0} stream,
                                                 PUBLIC.{1} lake 
                                          WHERE  stream.{2} = {4} 
                                          AND    st_intersects(lake.geom, stream.geom)) AS complex ) AS dumped ) AS intersection_data,
       PUBLIC.{0} stream_route 
WHERE  stream_route.{2} = {4}
";

            var sql = string.Format(linearReferenceString, streamTableName, geometryTableName, streamIdColumn,
                streamNameColumn, streamId, geometryIdColumn, geometryNameColumn);

            foreach (var linearReferenceResult in ExecuteQuery(sql))
            {
                yield return linearReferenceResult;
            }
        }

        private IEnumerable<LinearReferenceResult> ExecuteQuery(string sql)
        {
            var connection = String.Format("Server={0};Port=5432;User Id={1};Database={2};Password=fakepassword;", _dbConnection.HostName, _dbConnection.UserName, _dbConnection.DatabaseName);
            var conn = new NpgsqlConnection(connection);
            conn.Open();

            NpgsqlCommand command = new NpgsqlCommand(sql, conn);


            try
            {
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var l = new LinearReferenceResult();

                    // sometimes when a line intercepts a polygon you can
                    // get a point, e.g. the EXACT mouth of a river.
                    // ignore these non-line geometries.
                    l.IntersectionGeometryType = dr.GetString(6);
                    var isLineString = String.Equals("Linestring", l.IntersectionGeometryType,
                        StringComparison.OrdinalIgnoreCase);
                    if (isLineString == false)
                    {
                        yield break;
                    }

                    if (dr.IsDBNull(1))
                    {
                        yield break;
                    }

                    l.StreamId = dr.GetInt32(0);
                    l.StreamNumber = dr.IsDBNull(1) ? "Unnamed" : dr.GetString(1);
                    l.StreamName = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    l.GeometryName = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    var asdf = dr[4];
                    l.GeometryId = Convert.ToInt32(asdf);

                    l.IntersectionGeometry = dr.GetString(5);


                    l.StartPoint = dr.GetDouble(7);
                    l.EndPoint = dr.GetDouble(8);
                    l.StreamLength = dr.GetDouble(9);

                    yield return l;
                }
            }

            finally
            {
                conn.Close();
            }
        }


        public IEnumerable<Tuple<decimal, decimal>> GetOffsets(string streamId, string geometryTable)
        {
            const string linearReferenceString =
                @"SELECT (St_dump(St_intersection(palbuffer, strouter.geom))).geom                                                                 as thegeom,
       st_linelocatepoint(st_linemerge(strouter.geom), st_startpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom)) AS start,
       st_linelocatepoint(st_linemerge(strouter.geom), st_endpoint((st_dump(st_intersection(palbuffer, strouter.geom))).geom))   AS stop
FROM   streams_with_measured_kittle_routes strouter, 
       ( 
              SELECT st_buffer(st_union(dumpgeom), 3, 'endcap=flat join=round') AS palbuffer 
              FROM   ( 
                            SELECT pal.geom                        AS dumpgeom 
                            FROM   {1}        AS pal,
                            streams_with_measured_kittle_routes routes
                            WHERE  ST_Intersects(pal.geom, routes.geom) 
                            and routes.gid = '{0}') AS DUMP) AS palbuffer
WHERE  strouter.gid = '{0}'
";

            var connection = String.Format("Server={0};Port=5432;User Id={1};Database={2};Password=fakepassword;", _dbConnection.HostName, _dbConnection.UserName, _dbConnection.DatabaseName);
            var conn = new NpgsqlConnection(connection);
            conn.Open();

            var sql = string.Format(linearReferenceString, streamId, geometryTable);


            NpgsqlCommand command = new NpgsqlCommand(sql, conn);


            try
            {
                NpgsqlDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    var start = Convert.ToDecimal(dr[1]);
                    var stop = Convert.ToDecimal(dr[2]);

                    yield return new Tuple<decimal, decimal>(start, stop);

                }

            }

            finally
            {
                conn.Close();
            }
        }
    }
}