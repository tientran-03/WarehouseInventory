pipeline {
    agent any
    
    tools {
        dotnet 'dotnet-8.0'
        nodejs 'node-20'
    }
    
    environment {
        DOTNET_CLI_HOME = "${WORKSPACE}/.dotnet"
        PNPM_HOME = "${WORKSPACE}/.pnpm-store"
        PATH = "${PNPM_HOME}:${env.PATH}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build Backend') {
            steps {
                dir('backend') {
                    sh 'dotnet restore'
                    sh 'dotnet build --configuration Release'
                }
            }
        }
        
        stage('Build Frontend') {
            steps {
                dir('frontend') {
                    sh '''
                        npm install -g pnpm@11
                        pnpm install
                        pnpm build
                    '''
                }
            }
        }
        
        stage('Archive Artifacts') {
            steps {
                dir('backend/MultiWarehouseInventory.API/bin/Release/net8.0') {
                    archiveArtifacts artifacts: '**/*.dll', fingerprint: true
                }
                dir('frontend/dist') {
                    archiveArtifacts artifacts: '**/*', fingerprint: true
                }
            }
        }
        
        stage('Deploy - Staging') {
            when {
                branch 'develop'
            }
            steps {
                echo 'Deploy to staging environment'
                // Add your staging deployment commands here
                // Example: scp, docker push, etc.
            }
        }
        
        stage('Deploy - Production') {
            when {
                branch 'main'
            }
            input {
                message 'Deploy to production?'
                ok 'Yes, deploy'
            }
            steps {
                echo 'Deploy to production environment'
                // Add your production deployment commands here
                // Example: scp, docker push, etc.
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
