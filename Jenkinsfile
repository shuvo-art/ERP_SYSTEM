pipeline {
    agent any

    environment {
        // Global Configuration
        AWS_REGION      = "us-east-1"
        ECR_REGISTRY    = "123456789012.dkr.ecr.us-east-1.amazonaws.com" // Update with real ID
        PROJECT_NAME    = "erp-system"
        
        // Dynamic Versioning
        IMAGE_TAG       = "${env.BUILD_NUMBER}-${env.GIT_COMMIT.take(7)}"
        
        // Credential IDs (Configured in Jenkins)
        AWS_CREDS_ID    = "aws-credentials"
        GITHUB_CREDS_ID = "github-pat"
    }

    stages {
        stage('Initialize') {
            steps {
                script {
                    echo "Initializing Pipeline for Branch: ${env.BRANCH_NAME}"
                    
                    // Branch-Specific Configuration
                    if (env.BRANCH_NAME == 'main' || env.BRANCH_NAME == 'master') {
                        env.DEPLOY_ENV = "prod"
                        env.K8S_NAMESPACE = "erp-prod"
                        env.TF_DIR = "terraform/live/prod"
                        env.ANSIBLE_INV = "ansible/inventory/prod/hosts.ini"
                        env.HELM_VALUES = "k8s/erp-system/values-prod.yaml"
                    } else {
                        env.DEPLOY_ENV = "dev"
                        env.K8S_NAMESPACE = "erp-dev"
                        env.TF_DIR = "terraform/live/dev"
                        env.ANSIBLE_INV = "ansible/inventory/dev/hosts.ini"
                        env.HELM_VALUES = "k8s/erp-system/values-dev.yaml"
                    }
                    
                    echo "Environment set to: ${env.DEPLOY_ENV}"
                    echo "Version Tag: ${env.IMAGE_TAG}"
                }
            }
        }

        stage('Checkout') {
            steps {
             checkout scm
            }
        }

        stage('Build & Push Images') {
            parallel {
                stage('Auth API') {
                    steps {
                        buildAndPush("auth-api", "src/Auth.Api")
                    }
                }
                stage('Product API') {
                    steps {
                        buildAndPush("product-api", "src/ProductApi.Api")
                    }
                }
                stage('Job API') {
                    steps {
                        buildAndPush("job-api", "src/JobApi.Api")
                    }
                }
                stage('Frontend') {
                    steps {
                        // Assuming frontend is in root or src/frontend
                        // buildAndPush("frontend", "src/Client") 
                        echo "Skipping frontend for now (verify path)"
                    }
                }
            }
        }

        stage('Infrastructure (Terraform)') {
            steps {
                withCredentials([[
                    $class: 'AmazonWebServicesCredentialsBinding',
                    credentialsId: "${AWS_CREDS_ID}",
                    accessKeyVariable: 'AWS_ACCESS_KEY_ID',
                    secretKeyVariable: 'AWS_SECRET_ACCESS_KEY'
                ]]) {
                    dir("${env.TF_DIR}") {
                        sh 'terraform init'
                        sh 'terraform validate'
                        // In real CI, Plan often requires approval. Here we auto-approve for dev/prod flow demo.
                        sh 'terraform plan -out=tfplan'
                        sh 'terraform apply -auto-approve tfplan'
                        
                        // Capture setup info needed for later stages
                        script {
                            env.EKS_CLUSTER_NAME = sh(script: 'terraform output -raw cluster_name', returnStdout: true).trim()
                        }
                    }
                }
            }
        }

        stage('Configuration (Ansible)') {
            steps {
                // Ensure Ansible can talk to the instances (SSH Keys / Connectivity)
                // This assumes Jenkins has SSH access to the nodes
                dir('ansible') {
                    sh "ansible-playbook -i ${env.ANSIBLE_INV} site.yml --extra-vars 'env=${env.DEPLOY_ENV}'"
                }
            }
        }

        stage('Deployment (Helm)') {
            steps {
                withCredentials([[
                    $class: 'AmazonWebServicesCredentialsBinding',
                    credentialsId: "${AWS_CREDS_ID}",
                    accessKeyVariable: 'AWS_ACCESS_KEY_ID',
                    secretKeyVariable: 'AWS_SECRET_ACCESS_KEY'
                ]]) {
                    script {
                        // Connect to EKS
                        sh "aws eks update-kubeconfig --region ${AWS_REGION} --name ${env.EKS_CLUSTER_NAME}"
                        
                        // Deploy/Upgrade Chart
                        sh """
                            helm upgrade --install ${PROJECT_NAME} ./k8s/erp-system \
                            --namespace ${env.K8S_NAMESPACE} \
                            --create-namespace \
                            -f ${env.HELM_VALUES} \
                            --set global.image.tag=${env.IMAGE_TAG}
                        """
                    }
                }
            }
        }

        stage('Monitoring & Logging') {
            steps {
                script {
                    echo "Verifying Monitoring Stack..."
                    // Standard Prometheus/Grafana Stack
                    sh """
                        helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
                        helm repo update
                        helm upgrade --install monitoring prometheus-community/kube-prometheus-stack \
                        --namespace monitoring \
                        --create-namespace \
                        --set grafana.enabled=true
                    """
                }
            }
        }
    }
    
    post {
        always {
            cleanWs()
            echo "Pipeline Finished."
        }
        success {
            echo "Deployment Successful!"
        }
        failure {
            echo "Deployment Failed. Please check logs."
        }
    }
}

// Helper Function for Docker Build & Push
def buildAndPush(serviceName, dockerfileDir) {
    script {
        docker.withRegistry("https://${env.ECR_REGISTRY}", "ecr:us-east-1:${env.AWS_CREDS_ID}") {
            def image = docker.build("${env.ECR_REGISTRY}/${env.PROJECT_NAME}-${serviceName}:${env.IMAGE_TAG}", "-f ${dockerfileDir}/Dockerfile .")
            image.push()
            image.push("latest")
        }
    }
}
