pipeline {
    agent any

    environment {
        DOCKER_REGISTRY = "your-docker-hub-username"
        AWS_REGION = "us-east-1"
        EKS_CLUSTER_NAME = "dev-erp-eks"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build & Push Images') {
            steps {
                script {
                    def services = [
                        [name: "auth-api", dir: "Auth.Api"],
                        [name: "product-api", dir: "ProductApi.Api"],
                        [name: "job-api", dir: "JobApi.Api"],
                        [name: "about-us-api", dir: "AboutUsApi.Api"]
                    ]
                    
                    for (service in services) {
                        echo "Building and Pushing ${service.name}..."
                        sh "docker build -t ${DOCKER_REGISTRY}/erp-${service.name}:latest -f src/${service.dir}/Dockerfile ."
                        sh "docker push ${DOCKER_REGISTRY}/erp-${service.name}:latest"
                    }
                }
            }
        }

        stage('Infrastructure (Terraform)') {
            steps {
                dir('terraform/live/dev') {
                    sh "terraform init"
                    sh "terraform plan -out=tfplan"
                    // sh "terraform apply -auto-approve tfplan"
                }
            }
        }

        stage('Deployment (Helm)') {
            steps {
                script {
                    sh "aws eks update-kubeconfig --region ${AWS_REGION} --name ${EKS_CLUSTER_NAME}"
                    sh "helm upgrade --install erp-system ./charts/erp-system --namespace dev --create-namespace"
                }
            }
        }

        stage('Monitoring & Logging') {
            steps {
                echo "Deploying Monitoring Stack..."
                sh "helm upgrade --install monitoring prometheus-community/kube-prometheus-stack --namespace monitoring --create-namespace"
            }
        }
    }
}
