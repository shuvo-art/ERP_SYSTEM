terraform {
  required_version = ">= 0.12"
}

provider "aws" {
  region = "eu-west-3"
}

provider "kubernetes" {

  host                   = data.aws_eks_cluster.myapp-cluster.endpoint
  token                  = data.aws_eks_cluster_auth.myapp-cluster.token
  cluster_ca_certificate = base64decode(data.aws_eks_cluster.myapp-cluster.certificate_authority.0.data)
}
