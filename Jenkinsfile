pipeline {
    agent any

    options {
        skipDefaultCheckout(true)
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out repository...'
                checkout scm
            }
        }

        stage('Validate repository structure') {
            steps {
                script {
                    def requiredPaths = [
                        'README.md',
                        'docs/unity.md',
                        'docs/web_app.md',
                        'unity_game/AimTrainer/Assets',
                        'unity_game/AimTrainer/Packages',
                        'unity_game/AimTrainer/ProjectSettings'
                    ]

                    requiredPaths.each { path ->
                        if (!fileExists(path)) {
                            error "Required path not found: ${path}"
                        }

                        echo "OK: ${path}"
                    }
                }
            }
        }

        stage('Validate Unity repository hygiene') {
            steps {
                sh '''
                    set -e

                    TRACKED_GENERATED_FILES=$(git ls-files | grep -E \
                    '(^|/)(Library|Temp|Obj|Build|Builds|Logs|UserSettings)/' || true)

                    if [ -n "$TRACKED_GENERATED_FILES" ]; then
                        echo "ERROR: Unity generated files are being tracked:"
                        echo "$TRACKED_GENERATED_FILES"
                        exit 1
                    fi

                    echo "OK: no Unity generated folders are tracked."
                '''
            }
        }

        stage('Build information') {
            steps {
                sh '''
                    echo "Branch: $(git branch --show-current)"
                    echo "Commit: $(git rev-parse --short HEAD)"
                    echo "Message: $(git log -1 --pretty=%s)"
                '''
            }
        }
    }

    post {
        success {
            echo 'Repository validation completed successfully.'
        }

        failure {
            echo 'Repository validation failed.'
        }
    }
}