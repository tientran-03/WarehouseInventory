pipeline {
    agent any
    
    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/tientran-03/WarehouseInventory.git'
            }
        }
        
        stage('Build Backend') {
            steps {
                dir('backend') {
                    bat 'dotnet --version'
                    bat 'dotnet restore'
                    bat 'dotnet build --configuration Release'
                }
            }
        }
        
        stage('Archive Artifacts') {
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
