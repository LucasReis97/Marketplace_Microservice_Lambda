provider "aws" {
	region = "sa-east-1"
}

resource "aws_sqs_queue" "received" {
  name						= "received"
  delay_seconds             = 0
  max_message_size          = 262144
  visibility_timeout_seconds= 30
  message_retention_seconds = 345600
  receive_wait_time_seconds = 0
  policy = <<POLICY
{
  "Version": "2008-10-17",
  "Id": "__default_policy_ID",
  "Statement": [
    {
      "Sid": "__owner_statement",
      "Effect": "Allow",
      "Principal": "*",
      "Action": [
        "SQS:*"
      ],
      "Resource": "arn:aws:sqs:sa-east-1:502028380405:received"
    }
  ]
}
POLICY
}

resource "aws_sqs_queue" "reserved" {
  name						= "reserved"
  delay_seconds             = 0
  max_message_size          = 262144
  visibility_timeout_seconds= 30
  message_retention_seconds = 345600
  receive_wait_time_seconds = 0
  policy = <<POLICY
{
  "Version": "2008-10-17",
  "Id": "__default_policy_ID",
  "Statement": [
    {
      "Sid": "__owner_statement",
      "Effect": "Allow",
      "Principal": "*",
      "Action": [
        "SQS:*"
      ],
      "Resource": "arn:aws:sqs:sa-east-1:502028380405:reserved"
    }
  ]
}
POLICY
}
resource "aws_sqs_queue" "paid" {
  name						= "paid"
  delay_seconds             = 0
  max_message_size          = 262144
  visibility_timeout_seconds= 30
  message_retention_seconds = 345600
  receive_wait_time_seconds = 0
  policy = <<POLICY
{
  "Version": "2008-10-17",
  "Id": "__default_policy_ID",
  "Statement": [
    {
      "Sid": "__owner_statement",
      "Effect": "Allow",
      "Principal": "*",
      "Action": [
        "SQS:*"
      ],
      "Resource": "arn:aws:sqs:sa-east-1:502028380405:paid"
    }
  ]
}
POLICY
}