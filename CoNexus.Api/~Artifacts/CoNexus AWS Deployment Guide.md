# Survey API - AWS Deployment Guide

## Overview

This guide covers deploying the .NET 9 Survey API to AWS using multiple deployment strategies, from quick start to production-ready configurations.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Options](#architecture-options)
3. [Option 1: AWS App Runner (Easiest)](#option-1-aws-app-runner-easiest)
4. [Option 2: AWS Elastic Beanstalk](#option-2-aws-elastic-beanstalk)
5. [Option 3: ECS with Fargate (Recommended)](#option-3-ecs-with-fargate-recommended)
6. [Option 4: EC2 with IIS](#option-4-ec2-with-iis)
7. [Database Setup (RDS SQL Server)](#database-setup-rds-sql-server)
8. [Configuration Management](#configuration-management)
9. [Security & Networking](#security--networking)
10. [Monitoring & Logging](#monitoring--logging)
11. [CI/CD Pipeline](#cicd-pipeline)
12. [Cost Optimization](#cost-optimization)

---

## Prerequisites

### Local Environment

```bash
# Install AWS CLI
# Windows (using winget)
winget install Amazon.AWSCLI

# Mac
brew install awscli

# Linux
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
sudo ./aws/install

# Configure AWS credentials
aws configure
# AWS Access Key ID: YOUR_ACCESS_KEY
# AWS Secret Access Key: YOUR_SECRET_KEY
# Default region: us-east-1
# Default output format: json
```

### Required Tools

- ✅ .NET 9 SDK
- ✅ Docker Desktop
- ✅ AWS CLI v2
- ✅ Git

### AWS Account Setup

1. Create IAM user with appropriate permissions
2. Enable MFA for security
3. Create access keys for programmatic access

---

## Architecture Options

### Comparison Matrix

| Option | Complexity | Cost | Scalability | Management | Best For |
|--------|-----------|------|-------------|------------|----------|
| **App Runner** | ⭐ Low | $$ | Auto | Minimal | Quick deploy, startups |
| **Elastic Beanstalk** | ⭐⭐ Medium | $$ | Good | Low | Traditional apps |
| **ECS Fargate** | ⭐⭐⭐ Medium-High | $$$ | Excellent | Medium | Production, microservices |
| **EC2 + IIS** | ⭐⭐⭐⭐ High | $ | Manual | High | Legacy, full control |

---

## Option 1: AWS App Runner (Easiest)

### Overview
AWS App Runner automatically builds and deploys containerized applications with minimal configuration.

### Step 1: Prepare Dockerfile

Create `Dockerfile` in your API project root:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["CoNexus.Api/CoNexus.Api.csproj", "CoNexus.Api/"]
COPY ["SurveyApi.Application/SurveyApi.Application.csproj", "SurveyApi.Application/"]
COPY ["SurveyApi.Domain/SurveyApi.Domain.csproj", "SurveyApi.Domain/"]
COPY ["SurveyApi.Infrastructure/SurveyApi.Infrastructure.csproj", "SurveyApi.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "CoNexus.Api/CoNexus.Api.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/CoNexus.Api"
RUN dotnet build "CoNexus.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "CoNexus.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

# Set environment for App Runner
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CoNexus.Api.dll"]
```

### Step 2: Create ECR Repository

```bash
# Create ECR repository
aws ecr create-repository \
    --repository-name survey-api \
    --region us-east-1

# Get login credentials
aws ecr get-login-password --region us-east-1 | \
    docker login --username AWS --password-stdin \
    YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```

### Step 3: Build and Push Docker Image

```bash
# Build image
docker build -t survey-api:latest .

# Tag image
docker tag survey-api:latest \
    YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/survey-api:latest

# Push to ECR
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/survey-api:latest
```

### Step 4: Create App Runner Service

```bash
# Create service using AWS CLI
aws apprunner create-service \
    --service-name survey-api \
    --source-configuration '{
        "ImageRepository": {
            "ImageIdentifier": "YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/survey-api:latest",
            "ImageRepositoryType": "ECR",
            "ImageConfiguration": {
                "Port": "8080",
                "RuntimeEnvironmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Production"
                }
            }
        },
        "AutoDeploymentsEnabled": true
    }' \
    --instance-configuration '{
        "Cpu": "1024",
        "Memory": "2048"
    }' \
    --region us-east-1
```

**Or using AWS Console:**

1. Go to AWS App Runner
2. Click "Create service"
3. Choose "Container registry" → ECR
4. Select your image
5. Configure:
   - Port: 8080
   - CPU: 1 vCPU
   - Memory: 2 GB
   - Environment variables (see Configuration section)
6. Click "Create & deploy"

### Step 5: Configure Environment Variables

Add via Console or CLI:

```bash
aws apprunner update-service \
    --service-arn YOUR_SERVICE_ARN \
    --source-configuration '{
        "ImageRepository": {
            "ImageConfiguration": {
                "RuntimeEnvironmentVariables": {
                    "ConnectionStrings__Default": "YOUR_RDS_CONNECTION_STRING",
                    "ASPNETCORE_ENVIRONMENT": "Production",
                    "CrmOrigin": "https://your-crm.com"
                }
            }
        }
    }'
```

### App Runner Pricing

- **vCPU**: $0.064/vCPU-hour
- **Memory**: $0.007/GB-hour
- Example: 1 vCPU + 2GB = ~$60/month (24/7)

---

## Option 2: AWS Elastic Beanstalk

### Overview
Elastic Beanstalk provides a platform for deploying .NET applications with minimal infrastructure management.

### Step 1: Install EB CLI

```bash
# Install EB CLI
pip install awsebcli

# Verify installation
eb --version
```

### Step 2: Initialize Elastic Beanstalk

```bash
# Navigate to your solution root
cd SurveyApi

# Initialize EB application
eb init -p "64bit Amazon Linux 2023 v3.1.3 running .NET 8" survey-api --region us-east-1

# Create environment
eb create survey-api-prod \
    --instance-type t3.medium \
    --database \
    --database.engine sqlserver-ex \
    --database.username admin
```

### Step 3: Configure Deployment

Create `.ebextensions/01_environment.config`:

```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    ConnectionStrings__Default: '`{"Fn::GetOptionSetting": {"Namespace": "aws:rds:dbinstance", "OptionName": "endpoint"}}`'
  aws:elasticbeanstalk:environment:proxy:
    ProxyServer: nginx
  aws:autoscaling:launchconfiguration:
    InstanceType: t3.medium
    IamInstanceProfile: aws-elasticbeanstalk-ec2-role
```

### Step 4: Deploy Application

```bash
# Build and publish
dotnet publish -c Release -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to Elastic Beanstalk
eb deploy

# Open application
eb open
```

### Step 5: Configure Auto Scaling

```bash
# Create auto scaling configuration
eb config

# Add to environment.yaml:
aws:autoscaling:asg:
  MinSize: 2
  MaxSize: 10
aws:autoscaling:trigger:
  MeasureName: CPUUtilization
  Statistic: Average
  Unit: Percent
  UpperThreshold: 70
  LowerThreshold: 30
```

---

## Option 3: ECS with Fargate (Recommended)

### Overview
Amazon ECS with Fargate provides serverless container orchestration for production workloads.

### Architecture Diagram

```
┌─────────────────────────────────────────────────┐
│                 Application Load Balancer       │
│              (survey-api-alb.aws.com)           │
└────────────────┬────────────────────────────────┘
                 │
         ┌───────┴────────┐
         │                │
    ┌────▼────┐      ┌────▼────┐
    │ ECS Task│      │ ECS Task│
    │ Fargate │      │ Fargate │
    └────┬────┘      └────┬────┘
         │                │
         └───────┬────────┘
                 │
         ┌───────▼────────┐
         │   RDS SQL      │
         │   Server       │
         └────────────────┘
```

### Step 1: Create VPC and Networking

```bash
# Create VPC
aws ec2 create-vpc \
    --cidr-block 10.0.0.0/16 \
    --tag-specifications 'ResourceType=vpc,Tags=[{Key=Name,Value=survey-api-vpc}]'

# Create subnets (public and private in 2 AZs)
# Public Subnet 1
aws ec2 create-subnet \
    --vpc-id vpc-xxxxxx \
    --cidr-block 10.0.1.0/24 \
    --availability-zone us-east-1a

# Public Subnet 2
aws ec2 create-subnet \
    --vpc-id vpc-xxxxxx \
    --cidr-block 10.0.2.0/24 \
    --availability-zone us-east-1b

# Private Subnet 1
aws ec2 create-subnet \
    --vpc-id vpc-xxxxxx \
    --cidr-block 10.0.10.0/24 \
    --availability-zone us-east-1a

# Private Subnet 2
aws ec2 create-subnet \
    --vpc-id vpc-xxxxxx \
    --cidr-block 10.0.11.0/24 \
    --availability-zone us-east-1b

# Create and attach Internet Gateway
aws ec2 create-internet-gateway
aws ec2 attach-internet-gateway \
    --vpc-id vpc-xxxxxx \
    --internet-gateway-id igw-xxxxxx
```

### Step 2: Create ECS Cluster

```bash
# Create ECS cluster
aws ecs create-cluster \
    --cluster-name survey-api-cluster \
    --capacity-providers FARGATE FARGATE_SPOT \
    --default-capacity-provider-strategy \
        capacityProvider=FARGATE,weight=1,base=1 \
        capacityProvider=FARGATE_SPOT,weight=4
```

### Step 3: Create Task Definition

Create `task-definition.json`:

```json
{
  "family": "survey-api-task",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::YOUR_ACCOUNT:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::YOUR_ACCOUNT:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "survey-api",
      "image": "YOUR_ACCOUNT.dkr.ecr.us-east-1.amazonaws.com/survey-api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "essential": true,
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__Default",
          "valueFrom": "arn:aws:secretsmanager:us-east-1:YOUR_ACCOUNT:secret:survey-api/db-connection"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/survey-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

Register task definition:

```bash
aws ecs register-task-definition \
    --cli-input-json file://task-definition.json
```

### Step 4: Create Application Load Balancer

```bash
# Create security group for ALB
aws ec2 create-security-group \
    --group-name survey-api-alb-sg \
    --description "Security group for Survey API ALB" \
    --vpc-id vpc-xxxxxx

# Allow HTTP/HTTPS traffic
aws ec2 authorize-security-group-ingress \
    --group-id sg-xxxxxx \
    --protocol tcp \
    --port 443 \
    --cidr 0.0.0.0/0

# Create Application Load Balancer
aws elbv2 create-load-balancer \
    --name survey-api-alb \
    --subnets subnet-public1 subnet-public2 \
    --security-groups sg-xxxxxx \
    --scheme internet-facing \
    --type application

# Create target group
aws elbv2 create-target-group \
    --name survey-api-tg \
    --protocol HTTP \
    --port 8080 \
    --vpc-id vpc-xxxxxx \
    --target-type ip \
    --health-check-path /health \
    --health-check-interval-seconds 30 \
    --health-check-timeout-seconds 5 \
    --healthy-threshold-count 2 \
    --unhealthy-threshold-count 3

# Create listener
aws elbv2 create-listener \
    --load-balancer-arn arn:aws:elasticloadbalancing:... \
    --protocol HTTPS \
    --port 443 \
    --certificates CertificateArn=arn:aws:acm:... \
    --default-actions Type=forward,TargetGroupArn=arn:aws:elasticloadbalancing:...
```

### Step 5: Create ECS Service

```bash
# Create security group for ECS tasks
aws ec2 create-security-group \
    --group-name survey-api-ecs-sg \
    --description "Security group for Survey API ECS tasks" \
    --vpc-id vpc-xxxxxx

# Allow traffic from ALB
aws ec2 authorize-security-group-ingress \
    --group-id sg-ecs-xxxxxx \
    --protocol tcp \
    --port 8080 \
    --source-group sg-alb-xxxxxx

# Create ECS service
aws ecs create-service \
    --cluster survey-api-cluster \
    --service-name survey-api-service \
    --task-definition survey-api-task:1 \
    --desired-count 2 \
    --launch-type FARGATE \
    --network-configuration "awsvpcConfiguration={
        subnets=[subnet-private1,subnet-private2],
        securityGroups=[sg-ecs-xxxxxx],
        assignPublicIp=DISABLED
    }" \
    --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:...,containerName=survey-api,containerPort=8080" \
    --health-check-grace-period-seconds 60
```

### Step 6: Configure Auto Scaling

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
    --service-namespace ecs \
    --resource-id service/survey-api-cluster/survey-api-service \
    --scalable-dimension ecs:service:DesiredCount \
    --min-capacity 2 \
    --max-capacity 10

# Create scaling policy (CPU-based)
aws application-autoscaling put-scaling-policy \
    --service-namespace ecs \
    --resource-id service/survey-api-cluster/survey-api-service \
    --scalable-dimension ecs:service:DesiredCount \
    --policy-name cpu-scaling-policy \
    --policy-type TargetTrackingScaling \
    --target-tracking-scaling-policy-configuration '{
        "TargetValue": 70.0,
        "PredefinedMetricSpecification": {
            "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
        },
        "ScaleInCooldown": 300,
        "ScaleOutCooldown": 60
    }'
```

### ECS Fargate Pricing

**Per task per hour:**
- vCPU: $0.04048/vCPU
- Memory: $0.004445/GB

**Example (2 tasks, 0.5 vCPU, 1GB each):**
- Monthly: ~$60

---

## Option 4: EC2 with IIS

### Overview
Traditional deployment to Windows Server with IIS for organizations requiring full control.

### Step 1: Launch EC2 Instance

```bash
# Launch Windows Server instance
aws ec2 run-instances \
    --image-id ami-xxxxxxxxx \
    --instance-type t3.medium \
    --key-name your-key-pair \
    --security-group-ids sg-xxxxxx \
    --subnet-id subnet-xxxxxx \
    --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value=survey-api-server}]'
```

### Step 2: Connect and Configure IIS

```powershell
# Connect via RDP and run PowerShell as Administrator

# Install IIS and .NET 9 Hosting Bundle
Install-WindowsFeature -Name Web-Server -IncludeManagementTools

# Download and install .NET 9 Hosting Bundle
Invoke-WebRequest -Uri "https://download.visualstudio.microsoft.com/download/pr/.../dotnet-hosting-9.0-win.exe" -OutFile "dotnet-hosting.exe"
.\dotnet-hosting.exe /quiet /norestart

# Restart IIS
net stop was /y
net start w3svc
```

### Step 3: Deploy Application

```powershell
# Create application directory
New-Item -Path "C:\inetpub\survey-api" -ItemType Directory

# Copy published files (use FTP, RDP, or AWS Systems Manager)
# Assume files are in C:\deploy\

Copy-Item -Path "C:\deploy\*" -Destination "C:\inetpub\survey-api\" -Recurse

# Create IIS Application Pool
New-WebAppPool -Name "SurveyApiPool"
Set-ItemProperty IIS:\AppPools\SurveyApiPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\SurveyApiPool -Name processModel.identityType -Value ApplicationPoolIdentity

# Create IIS Website
New-Website -Name "SurveyApi" `
    -Port 80 `
    -PhysicalPath "C:\inetpub\survey-api" `
    -ApplicationPool "SurveyApiPool"

# Configure environment variables
$config = Get-WebConfiguration system.webServer/aspNetCore -PSPath "IIS:\Sites\SurveyApi"
$config.environmentVariables.Add("ASPNETCORE_ENVIRONMENT", "Production")
$config.environmentVariables.Add("ConnectionStrings__Default", "YOUR_CONNECTION_STRING")
```

### Step 4: Configure SSL Certificate

```powershell
# Install certificate (ACM or Let's Encrypt)
# Using AWS Certificate Manager with ALB is recommended

# Or use IIS certificate binding
New-WebBinding -Name "SurveyApi" -IP "*" -Port 443 -Protocol https
```

---

## Database Setup (RDS SQL Server)

### Step 1: Create RDS Subnet Group

```bash
aws rds create-db-subnet-group \
    --db-subnet-group-name survey-api-db-subnet \
    --db-subnet-group-description "Subnet group for Survey API database" \
    --subnet-ids subnet-private1 subnet-private2
```

### Step 2: Create RDS Security Group

```bash
# Create security group
aws ec2 create-security-group \
    --group-name survey-api-rds-sg \
    --description "Security group for Survey API RDS" \
    --vpc-id vpc-xxxxxx

# Allow SQL Server traffic from ECS tasks
aws ec2 authorize-security-group-ingress \
    --group-id sg-rds-xxxxxx \
    --protocol tcp \
    --port 1433 \
    --source-group sg-ecs-xxxxxx
```

### Step 3: Create RDS SQL Server Instance

```bash
aws rds create-db-instance \
    --db-instance-identifier survey-api-db \
    --db-instance-class db.t3.medium \
    --engine sqlserver-ex \
    --engine-version 15.00.4312.2.v1 \
    --master-username admin \
    --master-user-password 'YourSecurePassword123!' \
    --allocated-storage 100 \
    --storage-type gp3 \
    --storage-encrypted \
    --vpc-security-group-ids sg-rds-xxxxxx \
    --db-subnet-group-name survey-api-db-subnet \
    --backup-retention-period 7 \
    --preferred-backup-window "03:00-04:00" \
    --preferred-maintenance-window "mon:04:00-mon:05:00" \
    --multi-az \
    --publicly-accessible false \
    --tags Key=Name,Value=survey-api-database
```

### Step 4: Store Connection String in Secrets Manager

```bash
# Create secret
aws secretsmanager create-secret \
    --name survey-api/db-connection \
    --description "Survey API database connection string" \
    --secret-string "Server=survey-api-db.xxxxxxxxx.us-east-1.rds.amazonaws.com,1433;Database=SurveyDb;User Id=admin;Password=YourSecurePassword123!;Encrypt=true;TrustServerCertificate=true"
```

### Step 5: Run Migrations

```bash
# From your local machine or CI/CD pipeline
# Update connection string in appsettings.json temporarily

dotnet ef database update --project SurveyApi.Infrastructure --startup-project CoNexus.Api

# Or create SQL script and run via SQL Management Studio
dotnet ef migrations script --project SurveyApi.Infrastructure --startup-project CoNexus.Api --output migration.sql
```

### RDS Pricing Example

**SQL Server Express Edition:**
- db.t3.medium: $0.126/hour (~$91/month)
- Storage (100GB gp3): $11.50/month
- **Total: ~$103/month**

---

## Configuration Management

### Using AWS Systems Manager Parameter Store

```bash
# Store parameters
aws ssm put-parameter \
    --name "/survey-api/prod/ConnectionStrings__Default" \
    --value "YOUR_CONNECTION_STRING" \
    --type "SecureString" \
    --tier "Standard"

aws ssm put-parameter \
    --name "/survey-api/prod/CrmOrigin" \
    --value "https://your-crm.com" \
    --type "String"
```

### Using AWS Secrets Manager (Recommended)

```bash
# Create secret with multiple key-value pairs
aws secretsmanager create-secret \
    --name survey-api/config \
    --secret-string '{
        "ConnectionStrings__Default":"Server=...",
        "CrmOrigin":"https://your-crm.com",
        "JwtSecret":"your-jwt-secret"
    }'
```

### Access from Application

Update `Program.cs`:

```csharp
// Add AWS Systems Manager configuration
builder.Configuration.AddSystemsManager("/survey-api/prod");

// Or add Secrets Manager
builder.Configuration.AddSecretsManager(configurator: options =>
{
    options.SecretFilter = entry => entry.Name.StartsWith("survey-api/");
    options.KeyGenerator = (entry, key) => key.Replace("__", ":");
});
```

Install required package:

```bash
dotnet add package Amazon.Extensions.Configuration.SystemsManager
# or
dotnet add package Kralizek.Extensions.Configuration.AWSSecretsManager
```

---

## Security & Networking

### Security Groups Configuration

```
ALB Security Group (sg-alb):
- Inbound: 443 from 0.0.0.0/0
- Outbound: 8080 to sg-ecs

ECS Security Group (sg-ecs):
- Inbound: 8080 from sg-alb
- Outbound: 1433 to sg-rds, 443 to 0.0.0.0/0

RDS Security Group (sg-rds):
- Inbound: 1433 from sg-ecs
- Outbound: None
```

### IAM Roles

**ECS Task Execution Role:**

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "logs:CreateLogStream",
        "logs:PutLogEvents",
        "secretsmanager:GetSecretValue"
      ],
      "Resource": "*"
    }
  ]
}
```

**ECS Task Role (Application Permissions):**

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject"
      ],
      "Resource": "arn:aws:s3:::survey-api-assets/*"
    },
    {
      "Effect": "Allow",
      "Action": [
        "ses:SendEmail"
      ],
      "Resource": "*"
    }
  ]
}
```

### SSL/TLS Certificate

```bash
# Request certificate from ACM
aws acm request-certificate \
    --domain-name api.yourcompany.com \
    --subject-alternative-names *.yourcompany.com \
    --validation-method DNS \
    --region us-east-1

# After DNS validation, attach to ALB listener
aws elbv2 modify-listener \
    --listener-arn arn:aws:elasticloadbalancing:... \
    --certificates CertificateArn=arn:aws:acm:...
```

---

## Monitoring & Logging

### CloudWatch Logs

```bash
# Create log group
aws logs create-log-group \
    --log-group-name /ecs/survey-api

# Set retention
aws logs put-retention-policy \
    --log-group-name /ecs/survey-api \
    --retention-in-days 30
```

### CloudWatch Alarms

```bash
# High CPU alarm
aws cloudwatch put-metric-alarm \
    --alarm-name survey-api-high-cpu \
    --alarm-description "Survey API CPU over 80%" \
    --metric-name CPUUtilization \
    --namespace AWS/ECS \
    --statistic Average \
    --period 300 \
    --evaluation-periods 2 \
    --threshold 80 \
    --comparison-operator GreaterThanThreshold \
    --dimensions Name=ServiceName,Value=survey-api-service Name=ClusterName,Value=survey-api-cluster

# High memory alarm
aws cloudwatch put-metric-alarm \
    --alarm-name survey-api-high-memory \
    --alarm-description "Survey API memory over 80%" \
    --metric-name MemoryUtilization \
    --namespace AWS/ECS \
    --statistic Average \
    --period 300 \
    --evaluation-periods 2 \
    --threshold 80 \
    --comparison-operator GreaterThanThreshold

# 5xx errors alarm
aws cloudwatch put-metric-alarm \
    --alarm-name survey-api-5xx-errors \
    --alarm-description "Survey API 5xx errors" \
    --metric-name HTTPCode_Target_5XX_Count \
    --namespace AWS/ApplicationELB \
    --statistic Sum \
    --period 60 \
    --evaluation-periods 2 \
    --threshold 10 \
    --comparison-operator GreaterThanThreshold
```

### Application Insights

Add to `Program.cs`:

```csharp
// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

Install package:

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

---

## CI/CD Pipeline

### Using AWS CodePipeline + CodeBuild

#### buildspec.yml

```yaml
version: 0.2

phases:
  pre_build:
    commands:
      - echo Logging in to Amazon ECR...
      - aws ecr get-login-password --region $AWS_DEFAULT_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com
      - REPOSITORY_URI=$AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com/survey-api
      - COMMIT_HASH=$(echo $CODEBUILD_RESOLVED_SOURCE_VERSION | cut -c 1-7)
      - IMAGE_TAG=${COMMIT_HASH:=latest}
  build:
    commands:
      - echo Build started on `date`
      - echo Building the Docker image...
      - docker build -t $REPOSITORY_URI:latest .
      - docker tag $REPOSITORY_URI:latest $REPOSITORY_URI:$IMAGE_TAG
  post_build:
    commands:
      - echo Build completed on `date`
      - echo Pushing the Docker images...
      - docker push $REPOSITORY_URI:latest
      - docker push $REPOSITORY_URI:$IMAGE_TAG
      - echo Writing image definitions file...
      - printf '[{"name":"survey-api","imageUri":"%s"}]' $REPOSITORY_URI:$IMAGE_TAG > imagedefinitions.json

artifacts:
  files:
    - imagedefinitions.json
```

#### Create CodeBuild Project

```bash
aws codebuild create-project \
    --name survey-api-build \
    --source type=GITHUB,location=https://github.com/yourorg/survey-api.git \
    --artifacts type=S3,location=survey-api-artifacts \
    --environment type=LINUX_CONTAINER,image=aws/codebuild/standard:7.0,computeType=BUILD_GENERAL1_SMALL \
    --service-role arn:aws:iam::YOUR_ACCOUNT:role/CodeBuildServiceRole
```

#### Create CodePipeline

```bash
aws codepipeline create-pipeline --cli-input-json file://pipeline.json
```

**pipeline.json:**

```json
{
  "pipeline": {
    "name": "survey-api-pipeline",
    "roleArn": "arn:aws:iam::YOUR_ACCOUNT:role/CodePipelineServiceRole",
    "stages": [
      {
        "name": "Source",
        "actions": [
          {
            "name": "Source",
            "actionTypeId": {
              "category": "Source",
              "owner": "ThirdParty",
              "provider": "GitHub",
              "version": "1"
            },
            "configuration": {
              "Owner": "yourorg",
              "Repo": "survey-api",
              "Branch": "main",
              "OAuthToken": "{{resolve:secretsmanager:github-token}}"
            },
            "outputArtifacts": [{"name": "SourceOutput"}]
          }
        ]
      },
      {
        "name": "Build",
        "actions": [
          {
            "name": "Build",
            "actionTypeId": {
              "category": "Build",
              "owner": "AWS",
              "provider": "CodeBuild",
              "version": "1"
            },
            "configuration": {
              "ProjectName": "survey-api-build"
            },
            "inputArtifacts": [{"name": "SourceOutput"}],
            "outputArtifacts": [{"name": "BuildOutput"}]
          }
        ]
      },
      {
        "name": "Deploy",
        "actions": [
          {
            "name": "Deploy",
            "actionTypeId": {
              "category": "Deploy",
              "owner": "AWS",
              "provider": "ECS",
              "version": "1"
            },
            "configuration": {
              "ClusterName": "survey-api-cluster",
              "ServiceName": "survey-api-service",
              "FileName": "imagedefinitions.json"
            },
            "inputArtifacts": [{"name": "BuildOutput"}]
          }
        ]
      }
    ]
  }
}
```

### Using GitHub Actions

**.github/workflows/deploy.yml:**

```yaml
name: Deploy to AWS

on:
  push:
    branches: [main]

env:
  AWS_REGION: us-east-1
  ECR_REPOSITORY: survey-api
  ECS_SERVICE: survey-api-service
  ECS_CLUSTER: survey-api-cluster
  ECS_TASK_DEFINITION: task-definition.json
  CONTAINER_NAME: survey-api

jobs:
  deploy:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v3

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v2
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.AWS_REGION }}

      - name: Login to Amazon ECR
        id: login-ecr
        uses: aws-actions/amazon-ecr-login@v1

      - name: Build, tag, and push image to Amazon ECR
        id: build-image
        env:
          ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
          IMAGE_TAG: ${{ github.sha }}
        run: |
          docker build -t $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG .
          docker push $ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG
          echo "image=$ECR_REGISTRY/$ECR_REPOSITORY:$IMAGE_TAG" >> $GITHUB_OUTPUT

      - name: Fill in the new image ID in the Amazon ECS task definition
        id: task-def
        uses: aws-actions/amazon-ecs-render-task-definition@v1
        with:
          task-definition: ${{ env.ECS_TASK_DEFINITION }}
          container-name: ${{ env.CONTAINER_NAME }}
          image: ${{ steps.build-image.outputs.image }}

      - name: Deploy Amazon ECS task definition
        uses: aws-actions/amazon-ecs-deploy-task-definition@v1
        with:
          task-definition: ${{ steps.task-def.outputs.task-definition }}
          service: ${{ env.ECS_SERVICE }}
          cluster: ${{ env.ECS_CLUSTER }}
          wait-for-service-stability: true
```

---

## Cost Optimization

### Recommendations

1. **Use Fargate Spot for non-production**
   - 70% savings on compute costs
   - Mix with regular Fargate for stability

2. **Right-size RDS instances**
   - Start with db.t3.medium
   - Use Performance Insights to monitor
   - Consider Aurora Serverless for variable workloads

3. **Enable S3 lifecycle policies**
   - Move logs to Glacier after 90 days
   - Delete after 1 year

4. **Use Reserved Instances for predictable workloads**
   - 1-year commitment: 30-40% savings
   - 3-year commitment: 50-60% savings

5. **Implement auto-scaling**
   - Scale down during off-hours
   - Use target tracking for efficiency

### Monthly Cost Estimate (Production)

| Service | Configuration | Monthly Cost |
|---------|--------------|--------------|
| ECS Fargate | 2 tasks, 0.5 vCPU, 1GB | $60 |
| RDS SQL Server | db.t3.medium, Multi-AZ | $180 |
| Application Load Balancer | 1 ALB | $23 |
| NAT Gateway | 2 AZs | $90 |
| CloudWatch Logs | 10GB/month | $5 |
| Secrets Manager | 5 secrets | $2 |
| **Total** | | **~$360/month** |

### Cost Reduction Tips

- Use VPC endpoints to avoid NAT Gateway costs
- Consolidate CloudWatch log groups
- Use S3 for static assets with CloudFront
- Enable Cost Explorer and set budget alerts

---

## Deployment Checklist

### Pre-Deployment

- [ ] Code committed to repository
- [ ] Database migrations tested
- [ ] Environment variables documented
- [ ] SSL certificate obtained
- [ ] DNS records prepared
- [ ] Backup strategy defined
- [ ] Monitoring configured
- [ ] Security groups reviewed

### Deployment

- [ ] ECR repository created
- [ ] Docker image built and pushed
- [ ] RDS instance created and configured
- [ ] Secrets stored in Secrets Manager
- [ ] ECS cluster and service created
- [ ] Load balancer configured
- [ ] Auto-scaling policies set
- [ ] CI/CD pipeline established

### Post-Deployment

- [ ] Application accessible via HTTPS
- [ ] Health checks passing
- [ ] Logs appearing in CloudWatch
- [ ] Alarms triggered correctly
- [ ] Database connections working
- [ ] Performance baseline established
- [ ] Documentation updated
- [ ] Team trained on operations

---

## Troubleshooting

### Common Issues

**Issue: Tasks fail to start**
```bash
# Check task logs
aws logs tail /ecs/survey-api --follow

# Check task definition
aws ecs describe-task-definition --task-definition survey-api-task

# Check service events
aws ecs describe-services --cluster survey-api-cluster --services survey-api-service
```

**Issue: Cannot connect to RDS**
```bash
# Verify security group rules
aws ec2 describe-security-groups --group-ids sg-rds-xxxxxx

# Test connection from ECS task
aws ecs execute-command \
    --cluster survey-api-cluster \
    --task task-id \
    --container survey-api \
    --interactive \
    --command "/bin/bash"
```

**Issue: High latency**
```bash
# Check ALB metrics
aws cloudwatch get-metric-statistics \
    --namespace AWS/ApplicationELB \
    --metric-name TargetResponseTime \
    --dimensions Name=LoadBalancer,Value=app/survey-api-alb/xxxxx \
    --start-time 2025-01-01T00:00:00Z \
    --end-time 2025-01-01T23:59:59Z \
    --period 3600 \
    --statistics Average
```

---

## Support & Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS App Runner Documentation](https://docs.aws.amazon.com/apprunner/)
- [AWS RDS SQL Server Documentation](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/CHAP_SQLServer.html)
- [.NET on AWS](https://aws.amazon.com/developer/language/net/)

---

## Summary

This guide covered four deployment options for the Survey API:

1. **AWS App Runner** - Simplest, ideal for quick deployments
2. **Elastic Beanstalk** - Managed platform for .NET apps
3. **ECS Fargate** - Production-ready, scalable containers
4. **EC2 + IIS** - Traditional Windows hosting

Choose based on your requirements for control, scalability, and operational complexity. For most production scenarios, **ECS with Fargate** provides the best balance of features, scalability, and manageability.