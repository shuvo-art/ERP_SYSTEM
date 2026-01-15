# Ansible Deployment for ERP System

This directory contains the Ansible configuration for deploying the ERP System to Dev, Staging, and Production environments.

## Directory Structure

- `inventory/`: Contains environment-specific inventories and variables.
  - `dev/`: Development environment configuration.
  - `staging/`: Staging environment configuration.
  - `prod/`: Production environment configuration.
- `roles/`: Ansible roles containing tasks, templates, and handlers.
  - `common`: Basic server setup (packages, users).
  - `docker`: Installs Docker and configures users.
  - `eks-config`: Configures kubectl and EKS access.
- `site.yml`: The main playbook entry point.
- `ansible.cfg`: Global Ansible configuration.

## Usage

### Development
```bash
ansible-playbook -i inventory/dev/hosts.ini site.yml
```

### Staging
```bash
ansible-playbook -i inventory/staging/hosts.ini site.yml
```

### Production
```bash
ansible-playbook -i inventory/prod/hosts.ini site.yml
```

## Variables

- **Global Variables**: Defined in `group_vars/all.yml` (e.g., project name).
- **Environment Variables**: Defined in `inventory/<env>/group_vars/all.yml` (e.g., database credentials, debug settings).

## Best Practices Followed

- **Directory Layout**: Standard Ansible layout separating roles, inventories, and playbooks.
- **Environment Isolation**: Separate inventory directories for each environment ensures isolation.
- **Variable Precedence**: using `group_vars` within inventory ensures environment-specific overrides.
- **Modular Roles**: Reusable logic encapsulated in roles.
