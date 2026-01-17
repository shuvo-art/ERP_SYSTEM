provider "aws" {
  region = "us-east-1"
}

# Global ECR Repository
# This is in 'common' because build artifacts are typically built ONCE
# and promoted through Dev -> Staging -> Prod environments.

resource "aws_ecr_repository" "erp_app" {
  name                 = "erp-system-app"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Environment = "global"
    Project     = "erp-system"
  }
}

output "repository_url" {
  value = aws_ecr_repository.erp_app.repository_url
}
