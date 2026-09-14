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
                // Ép Docker lấy thư mục backend làm context để khớp hoàn toàn với các lệnh COPY trong Dockerfile
                sh 'docker build -t mwi-api:latest -f docker/backend/Dockerfile ./backend'
            }
        }
        
        stage('Archive Artifacts') {
            steps {
                echo 'Build Docker image completed successfully!'
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