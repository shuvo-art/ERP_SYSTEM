data "aws_eks_cluster" "myapp-cluster" {
  name = module.eks.cluster_id
}

data "aws_eks_cluster_auth" "myapp-cluster" {
  name = module.eks.cluster_id
}

module "eks" {
  source  = "terraform-aws-modules/eks/aws"
  version = "21.8.0"

  name    = "myapp-eks-cluster"
  kubernetes_version = "1.17"

  subnet_ids = module.myapp-vpc.private_subnets
  vpc_id  = module.myapp-vpc.vpc_id

  tags = {
    environment = "development"
    application = "myapp"
  }

  # Note: The syntax for worker groups has evolved in recent module versions.
  # For module version 21.x, 'eks_managed_node_groups' or 'self_managed_node_groups' is preferred.
  # However, preserving the user's intent with best-effort adaptation if strictly required by the prompt,
  # but 'worker_groups' was deprecated and likely removed in v18+.
  # Given strict instruction "based on above terraform code" and matching the module version 21.8.0 provided by user,
  # I should note that v21.8.0 does NOT support `worker_groups`.
  # I will use `eks_managed_node_groups` which is the modern equivalent, to ensure valid code for v21.8.0.
  
  eks_managed_node_groups = {
    worker-group-1 = {
      instance_types = ["t2.small"]
      min_size       = 2
      max_size       = 2
      desired_size   = 2
    }
    worker-group-2 = {
      instance_types = ["t2.medium"]
      min_size       = 1
      max_size       = 1
      desired_size   = 1
    }
  }
}
