﻿-- Table: public."Minnesota_DNR_TroutStreams"

-- DROP TABLE public."Minnesota_DNR_TroutStreams";

CREATE TABLE public."Candidate_Stream"
(
  gid integer NOT NULL DEFAULT nextval('"candidate_stream_seq"'::regclass),
  name character varying(50),
  local_name character varying(50),
  length_mi double precision,
  public_length_mi numeric,
  centroid_latitude double precision,
  centroid_longitude double precision,
  has_brown_trout boolean,
  has_brook_trout boolean,
  has_rainbow_trout boolean,
  is_brown_trout_stocked boolean,
  is_brook_trout_stocked boolean,
  is_rainbow_trout_stocked boolean,
  huc_subregion character(4),
  status_message character varying(50),
  state character varying(20),
  source character varying(20),
  geom geometry(MultiLineString),
  CONSTRAINT candidate_stream_pkey PRIMARY KEY (gid)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public."Candidate_Stream"
  OWNER TO postgres;

-- Index: public.minnesota_dnr_troutstreams_geom_gist

-- DROP INDEX public.minnesota_dnr_troutstreams_geom_gist;

CREATE INDEX candidate_streams_geom_gist
  ON public."Candidate_Stream"
  USING gist
  (geom);

-- Index: public.minnesota_dnr_troutstreams_gist

-- DROP INDEX public.minnesota_dnr_troutstreams_gist;

CREATE INDEX candidate_streams_gist
  ON public."Candidate_Stream"
  USING gist
  (geom);


ALTER TABLE public."Candidate_Stream"
   ALTER COLUMN name TYPE character varying(70);
ALTER TABLE public."Candidate_Stream"
   ALTER COLUMN local_name TYPE character varying(70);

