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
                    sh 'which dotnet || echo "dotnet not found in PATH"'
                    sh 'dotnet --version || echo "dotnet version check failed"'
                    sh 'dotnet restore'
                    sh 'dotnet build --configuration Release'
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
