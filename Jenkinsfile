pipeline {
    agent any
    
    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/tientran-03/WarehouseInventory.git'
            }
        }
        
        stage('Build Backend') {
            agent {
                docker {
                    image 'mcr.microsoft.com/dotnet/sdk:8.0'
                    args '-u root'
                }
            }
            steps {
                dir('backend') {
                    sh 'dotnet restore'
                    sh 'dotnet build --configuration Release'
                }
            }
        }
        
        stage('Archive Artifacts') {
            agent any
            steps {
                dir('backend/MultiWarehouseInventory.API/bin/Release/net8.0') {
                    archiveArtifacts artifacts: '**/*.dll', fingerprint: true
                }
            }
        }
    }
    
    post {
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
        always {
            cleanWs()
        }
    }
}