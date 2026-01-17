variable "cluster_name" {
  description = "Name of the EKS cluster"
  type        = string
}

variable "cluster_version" {
  description = "Cluster version"
  type        = string
  default     = "1.30"
}

variable "tags" {
  description = "Tags to apply to addons"
  type        = map(string)
  default     = {}
}
