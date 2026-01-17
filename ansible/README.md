# Server Configuration Management (Ansible)

This directory contains the Ansible configuration for provisioning, maintaining, and managing the servers hosting the ERP System. It follows industry best practices for directory layout, role separation, and environment isolation.

## 🏗 Directory Structure

```text
ansible/
├── ansible.cfg                # Global Ansible Settings
├── site.yml                   # Main Orchestrator Playbook
├── inventory/                 # Environment Inventories
│   ├── dev/                   # Development (Localhost/Dev Servers)
│   ├── staging/               # Staging (Pre-Prod)
│   └── prod/                  # Production (Live)
├── playbooks/                 # Operational Playbooks
│   ├── provision.yml          # Initial Server Setup (Docker, Users)
│   ├── maintenance.yml        # Patching & Updates
│   └── health_check.yml       # Read-only Status Checks
└── roles/                     # Reusable Logic Units
    ├── common/                # Base packages (git, curl, vim)
    ├── docker/                # Docker Engine & Compose setup
    └── eks-config/            # Kubernetes Client Tools (kubectl)
```

## 🚀 Usage

### 1. Provisioning (Initial Setup)
Run this when you spin up new servers to install Docker and base dependencies.

**Development:**
```bash
ansible-playbook -i inventory/dev/hosts.ini playbooks/provision.yml
```

**Production:**
```bash
ansible-playbook -i inventory/prod/hosts.ini playbooks/provision.yml
```

### 2. Maintenance (Patching)
Run this to apply security updates and safely reboot servers if needed.

```bash
ansible-playbook -i inventory/prod/hosts.ini playbooks/maintenance.yml
```

### 3. Health Checks
Run this to verify disk usage, memory, and service status without making changes.

```bash
ansible-playbook -i inventory/prod/hosts.ini playbooks/health_check.yml
```

## 🔑 Key Concepts

- **Split Inventory**: We do not mix Dev and Prod hosts. Each environment has its own folder in `inventory/` with its own `group_vars`.
- **Roles**: All tasks are encapsulated in roles. `site.yml` and `provision.yml` simply glue these roles together.
- **Tags**: Playbooks use tags (e.g., `ansible-playbook ... --tags "docker"`) to run specific parts of the configuration.
- **Idempotency**: All tasks are designed to be run multiple times without causing side effects.
