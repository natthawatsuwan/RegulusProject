FROM jenkins/jenkins:lts
USER root

# Update package list and install dependencies
RUN apt-get update && apt-get install -y \
    sudo \
    apt-transport-https \
    ca-certificates \
    curl \
    software-properties-common

# Add Docker's official GPG key
RUN curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo tee /etc/apt/trusted.gpg.d/docker.asc

# Set up Docker stable repository
RUN sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
RUN echo "deb [arch=amd64] https://download.docker.com/linux/debian $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list


# Update package index and install Docker
RUN apt-get update && apt-get install -y \
    docker-ce \
    docker-ce-cli \
    containerd.io

# Grant Jenkins user the necessary permissions to run Docker commands
RUN usermod -aG docker jenkins

# Allow Jenkins user to run Docker without sudo (by adding to Docker group)
RUN echo "jenkins ALL=NOPASSWD: /usr/bin/docker" >> /etc/sudoers

# Set Jenkins back to the jenkins user
USER jenkins

# Expose Jenkins port (optional)
EXPOSE 8080
