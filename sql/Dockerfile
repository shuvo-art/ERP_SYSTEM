FROM mcr.microsoft.com/mssql-tools

# User root to create directory
USER root
RUN mkdir -p /sql

# Copy all sql scripts
COPY . /sql/

# Switch back to non-root if necessary, but mssql-tools often runs as root
WORKDIR /sql

# The entrypoint and command will be handled in docker-compose or can be defined here
ENTRYPOINT ["/bin/bash"]
