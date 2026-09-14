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
        sh 'docker build -t mwi-api:latest -f docker/backend/Dockerfile .'
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