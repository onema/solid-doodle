# λ# Query and Visualize CloudWatch Logs w/ Lambda and Athena - August 2017 Team Hackathon Challenge
Traditional relational databases are still very prevalent today. Often however, we need to search or slice and dice the data many different ways and RDBMS's are not great at that. As many do, will be using [ElasticSearch](https://aws.amazon.com/elasticsearch-service/) to provide this functionality. Today we will explore how AWS Lambda can help us index our relational data into ElasticSearch with minimal effort and cost. We will be using [Amazon Aurora](https://aws.amazon.com/rds/aurora/) (MySQL), [Amazon Lambda](https://aws.amazon.com/lambda/), and [ElasticSearch](https://aws.amazon.com/elasticsearch-service/). 

We will setup database triggers to fire a lambda function when an INSERT, UPDATE or DELETE occurs in Aurora. The lambda function will then be responsible for updating the ElasticSearch index.

### Pre-requisites
The following tools and accounts are required to complete these instructions.

* [Install .NET Core 1.x](https://www.microsoft.com/net/core)
* [Install AWS CLI](https://aws.amazon.com/cli/)
* [Sign-up for an AWS account](https://aws.amazon.com/)
* [Sign-up for an Amazon developer account](https://developer.amazon.com/)
* [Twitter account](https://twitter.com/)

## LEVEL 0 - Setup
The following steps will walk you through the set-up of a CloudWatch Log and the provided lambda function that will be used to parse the log stream.

### Create a Twitter account
1. Sign into twitter and navigate to [https://apps.twitter.com/](https://apps.twitter.com/)
2. Click on [Create New App](https://apps.twitter.com/app/new)
3. Fill in the name, description, and a placeholder url and create the twitter application
4. Navigate to `Keys and Access Tokens` tab and click `Create my access token`
5. Save the 
    1. Consumer Key
    2. Consumer Secret
    3. Access Token
    4. Access Token Secret
6. Navigate to `/LogGenerator/credentials.json` and complete the fieds


### Create `lambdasharp` AWS Profile
The project uses by default the `lambdasharp` profile. Follow these steps to setup a new profile if need be.

1. Create a `lambdasharp` profile: `aws configure --profile lambdasharp`
2. Configure the profile with the AWS credentials and region you want to use

### Create a CloudWatch Log Group and Log Stream
Set up the CloudWatch Log Group `/lambda-sharp/log-parser/dev` and a log stream `test-log-stream` via the [AWS Console](https://console.aws.amazon.com/cloudwatch) or by executing AWS CLI commands.
```shell
aws logs create-log-group --log-group-name '/lambda-sharp/log-parser/dev'
aws logs create-log-stream --log-group-name '/lambda-sharp/log-parser/dev' --log-stream-name test-log-stream
```

### Create `LambdaSharp-LogParserRole` role for the lambda function
The `LambdaSharp-LogParserRole` lambda function requires an IAM role. You can create the `LambdaSharp-LogParserRole` role via the [AWS Console](https://console.aws.amazon.com/iam/home) or by executing [AWS CLI](https://aws.amazon.com/cli/) commands.
```shell
aws iam create-role --profile lambdasharp --role-name LambdaSharp-LogParserRole --assume-role-policy-document file://assets/lambda-role-policy.json
aws iam attach-role-policy --profile lambdasharp --role-name LambdaSharp-LogParserRole --policy-arn arn:aws:iam::aws:policy/AWSLambdaFullAccess
```

### Deploy the `LogParser` lambda function
1. Navigate into the LogGenerator folder: `cd LogParser`
2. Run: `dotnet restore`
3. Edit `aws-lambda-tools-defaults.json` and make sure everything is set up correctly.
5. Run: `dotnet lambda deploy-function`

## Level 1 - Stream data from Twitter into CloudWatch
Using the included LogGenerator project, stream tweets directly into CloudWatch

### Stream Tweets into the CloudWatch Log
The included LogGenerator project will stream live tweets directly into the CloudWatch log matching the above setup.

---

TODO: How will twitter credentials be provided?

---

1. Navigate into the LogGenerator folder: `cd LogGenerator`
2. Run: `dotnet restore`
3. Run: `dotnet run`

Data will begin streaming from Twitter into the CloudWatch log and will end after 30 seconds. The duration can be edited by modifying the `STREAM_DURATION` variable at the top of the Program.cs file.
> Note: CloudWatch log events must be sent to a log stream in chronological order. You will run into issues if multiple instances of the LogGenerator are streaming to the same log stream at the same time.

## Level 2 - Setup Lambda trigger and save data to S3

### Deploy lambda function
1. Navigate into the LogParser folder: `cd LogParser`
2. Run: `dotnet restore`
3. Run: `dotnet lambda deploy-function`

### Set up lambda trigger
1. From the AWS Console, navigate to the Lambda Services console
2. Find the deployed function and click into it to find the `Triggers` tab
3. Add a trigger and select `CloudWatch Logs` as the trigger
4. Select the log group `/lambda-sharp/log-parser/dev` and add a filter name

### Save data to S3

## Level 3 - Search log data using ElasticSearch
Use the lambda function to transform the streamed data into an ElasticSearch-readable JSON format, and search it from S3.

### Set up ElasticSearch
Set up an ElasticSearch index called `tweets`, and define its schema. The following is an minimal example, add additional fields for information found in the log streams.

---

TODO: Modify the below example for tweets!

---
```sql
CREATE DATABASE lambdasharp_logs;
```


```sql
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
```

```sql
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
```

### Use lambda to transform the log data
Extend the LogParser lambda function to transform CloudWatch log data into a ElasticSearch-readable JSON format.

## Boss Level
TBD

## Acknowledgements
* Erik Birkfeld for organizing.
* [MindTouch](https://mindtouch.com/) for hosting.

## Copyright & License
* Copyright (c) 2017 Juan Manuel Torres, Katherine Marino, Daniel Lee
* MIT License