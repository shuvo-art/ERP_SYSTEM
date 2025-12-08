pipeline {
    agent any

    environment {
        // Application
        APP_NAME = 'semcorp-api'
        DOCKER_REGISTRY = 'your-docker-registry' // e.g., docker.io/username or ECR URL
        IMAGE_TAG = "${env.BUILD_NUMBER}"
        
        // Terraform
        TF_WORKING_DIR = 'terraform'
        
        // Ansible
        ANSIBLE_WORKING_DIR = 'ansible'
        
        // Kubernetes
        K8S_WORKING_DIR = 'k8s'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build & Test') {
            steps {
                script {
                    echo 'Restoring dependencies...'
                    sh 'dotnet restore'
                    
                    echo 'Building application...'
                    sh 'dotnet build --no-restore --configuration Release'
                    
                    // Tests are currently empty, but this is where they would run
                    // echo 'Running tests...'
                    // sh 'dotnet test --no-build --configuration Release'
                }
            }
        }

        stage('Docker Build & Push') {
            steps {
                script {
                    docker.withRegistry('https://${DOCKER_REGISTRY}', 'docker-credentials') {
                        def appImage = docker.build("${DOCKER_REGISTRY}/${APP_NAME}:${IMAGE_TAG}")
                        appImage.push()
                        appImage.push('latest')
                    }
                }
            }
        }

        stage('Infrastructure Provisioning (Terraform)') {
            steps {
                script {
                    dir("${TF_WORKING_DIR}") {
                        withCredentials([usernamePassword(credentialsId: 'aws-credentials', passwordVariable: 'AWS_SECRET_ACCESS_KEY', usernameVariable: 'AWS_ACCESS_KEY_ID')]) {
                            sh 'terraform init'
                            sh 'terraform validate'
                            sh 'terraform plan -out=tfplan'
                            
                            input message: 'Apply Infrastructure Changes?', ok: 'Yes'
                            
                            sh 'terraform apply -auto-approve tfplan'
                        }
                    }
                }
            }
        }

        stage('Configuration Management (Ansible)') {
            steps {
                script {
                    dir("${ANSIBLE_WORKING_DIR}") {
                        sshagent(['ssh-credentials']) {
                            sh 'ansible-playbook -i inventory/hosts.ini site.yml'
                        }
                    }
                }
            }
        }

        stage('Deploy to Kubernetes') {
            steps {
                script {
                    dir("${K8S_WORKING_DIR}") {
                        withKubeConfig([credentialsId: 'kubeconfig-credentials']) {
                            // Update deployment with new image tag
                            sh "sed -i 's|${APP_NAME}:latest|${DOCKER_REGISTRY}/${APP_NAME}:${IMAGE_TAG}|g' 05-api.yaml"
                            
                            echo 'Deploying to Kubernetes...'
                            sh 'kubectl apply -f monitoring/'
                            sh 'kubectl apply -f .'
                            
                            // Verify rollout
                            sh 'kubectl rollout status deployment/order-api'
                        }
                    }
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            echo 'Pipeline completed successfully!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}
