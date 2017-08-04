#!/usr/bin/env bash

# Create a Role for the lambda function 
aws iam create-role --profile lambdasharp --role-name LambdaSharpLogParserRole --assume-role-policy-document file://assets/lambda-role-policy.json
aws iam attach-role-policy --profile lambdasharp --role-name LambdaSharpLogParserRole --policy-arn arn:aws:iam::aws:policy/AWSLambdaFullAccess

# Create log group and stream
aws logs create-log-group --log-group-name '/lambda-sharp/log-parser/dev'
aws logs create-log-stream --log-group-name '/lambda-sharp/log-parser/dev' --log-stream-name test-log-stream
