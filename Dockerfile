FROM jenkins/jenkins:lts

USER root

RUN apt-get update && \
    apt-get install -y python3 python3-pip python3-venv && \
    rm -rf /var/lib/apt/lists/*

COPY DevOps/json-validation/requirements.txt /tmp/json-requirements.txt

RUN python3 -m venv /opt/json-validator-venv && \
    /opt/json-validator-venv/bin/pip install --no-cache-dir \
    -r /tmp/json-requirements.txt

USER jenkins