
CREATE DATABASE lambdasharp_logs;


CREATE EXTERNAL TABLE IF NOT EXISTS lambdasharp_logs.users (
  `user_name` string,
  `name` string,
  `favorite` int,
  `tweet_count` int,
  `friends` int,
  `follow` int,
  `date_created` timestamp
)
ROW FORMAT SERDE 'org.openx.data.jsonserde.JsonSerDe'
WITH SERDEPROPERTIES (
  'serialization.format' = '1'
) LOCATION 's3://<USERNAME>-lambda-sharp-s3-logs/users/'
TBLPROPERTIES ('has_encrypted_data'='false');

CREATE EXTERNAL TABLE IF NOT EXISTS lambdasharp_logs.tweet_info (
  `user_name` string,
  `retweeted` int,
  `favorited` int,
  `message` string,
  `hashtags` array<string>,
  `latitude` double,
  `longitude` double,
  `date_created` timestamp
)
ROW FORMAT SERDE 'org.openx.data.jsonserde.JsonSerDe'
WITH SERDEPROPERTIES (
  'serialization.format' = '1'
) LOCATION 's3://<USERNAME>-lambda-sharp-s3-logs/tweet-info/'
TBLPROPERTIES ('has_encrypted_data'='false');
