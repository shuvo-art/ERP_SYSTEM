# Order Processing API (DevOps Project)

A robust, enterprise-grade RESTful API built with **.NET 8**, equipped with a complete DevOps lifecycle including Infrastructure as Code (Terraform), Configuration Management (Ansible), Container Orchestration (Kubernetes), Monitoring (Prometheus/Grafana), and CI/CD (Jenkins).

## 🚀 Features

### Application
- **Clean Architecture**: .NET 8 Web API separated into Api, Core, and Infrastructure layers.
- **High Performance**: Uses **Dapper** and **Stored Procedures** for rapid data access.
- **Reliability**: Implements **Idempotency** for safe retries of order creation.
- **Observability**: Exposes Prometheus metrics at `/metrics`.

### DevOps Stack
- **Infrastructure**: Terraform scripts to provision AWS EKS Cluster.
- **Configuration**: Ansible playbooks to provision Docker servers.
- **Orchestration**: Kubernetes manifests for Deployments, Services, ConfigMaps, and Secrets.
- **Monitoring**: Full Prometheus, Grafana, and Alertmanager stack.
- **Security**: SSL/TLS via Cert-Manager and NGINX Ingress; IdP-ready architecture.
- **CI/CD**: Declarative Jenkinsfile pipeline for Build -> Test -> Provision -> Deploy.

---

## 🛠 Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Terraform](https://www.terraform.io/downloads)
- [Ansible](https://docs.ansible.com/ansible/latest/installation_guide/intro_installation.html)
- [Jenkins](https://www.jenkins.io/) (for CI/CD)

---

## 📦 Quick Start (Local Docker Compose)

Run the full stack (API + SQL Server) locally:

```bash
docker compose up --build
```

**Verify API**:
```bash
curl -X POST http://localhost:8080/api/v1/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "totalAmount": 100.00,
    "requestId": "550e8400-e29b-41d4-a716-446655440001",
    "items": [{"productId": "P1", "quantity": 1, "unitPrice": 100}]
  }'
```

**Check Metrics**: `http://localhost:8080/metrics`

---

## ☁️ Infrastructure (Terraform)

Provision the AWS EKS cluster:

```bash
cd terraform
terraform init
terraform plan
terraform apply
```
*Note: Requires AWS Credentials configured.*

---

## ⚙️ Configuration (Ansible)

Provision a bare metal or VM server with Docker:

1.  Edit `ansible/inventory/hosts.ini` with your server IP.
2.  Run Playbook:
    ```bash
    ansible-playbook -i ansible/inventory/hosts.ini ansible/site.yml
    ```

---

## ☸️ Kubernetes Deployment

Deploy the application, database, monitoring, and SSL to your cluster.

### 1. Build and Push Image
```bash
docker build -t your-registry/semcorp-api:latest .
docker push your-registry/semcorp-api:latest
```

### 2. Base Deployment (DB + API)
```bash
kubectl apply -f k8s/
```

### 3. Monitoring Stack (Prometheus/Grafana)
```bash
kubectl apply -f k8s/monitoring/
```
- **Prometheus**: Port-forward 9090
- **Grafana**: Service LoadBalancer on port 3000

### 4. SSL/TLS (Cert-Manager)
*Prerequisite: Install NGINX Ingress Controller & Cert-Manager.*
```bash
# Update email in k8s/06-cluster-issuer.yaml first!
kubectl apply -f k8s/06-cluster-issuer.yaml
kubectl apply -f k8s/07-ingress.yaml
```

---

## 🔄 CI/CD Pipeline (Jenkins)

The `Jenkinsfile` at the root defines the pipeline:
1.  **Checkout**: Git clone.
2.  **Build**: .NET Build & Restore.
3.  **Docker**: Build image & Push to Registry.
4.  **Terraform**: Plan & Apply infrastructure changes (Manual Approval).
5.  **Ansible**: Configure servers.
6.  **Deploy**: Update Kubernetes manifests.

**Setup**: Add credentials (`docker-credentials`, `aws-credentials`, `kubeconfig-credentials`, `ssh-credentials`) to Jenkins.

---

## 📂 Project Structure

```
├── ansible/                # Ansible Playbooks & Roles
├── k8s/                    # Kubernetes Manifests
│   ├── monitoring/         # Prometheus/Grafana/Alertmanager
│   ├── 05-api.yaml         # App Deployment
│   └── 07-ingress.yaml     # SSL Ingress
├── src/                    # .NET 8 Source Code
├── sql/                    # SQL Init Scripts
├── terraform/              # AWS EKS IaC
├── Jenkinsfile             # CI/CD Pipeline Definition
├── docker-compose.yaml     # Local Development
└── deployment_architecture.md # Detailed Architecture & Strategy
```

---

## Part 3: Code Review & Mentorship Simulation

We conducted a review of a junior developer's code snippet for fetching orders. Below are the critical findings and the proposed solution.

### 🚩 Critical Issues Identified

#### 1. Security: SQL Injection Vulnerability 🔓
- **Issue**: The code concatenates the user-provided `orderId` directly into the SQL query string (`"SELECT ... WHERE OrderId = '" + orderId + "'"`).
- **Risk**: Allows **SQL Injection** attacks. A malicious user could alter the query (e.g., `' OR 1=1 --`) to expose or destroy data.
- **Fix**: Use **Parameterized Queries** (e.g., `@OrderId`) to treat input as data, not executable code.

#### 2. Security: Hardcoded Credentials 🔑
- **Issue**: Database credentials (`User Id=...;Password=...`) are explicitly written in the source code.
- **Risk**: Credentials exposed in source control. If the repo is compromised, the database is compromised.
- **Fix**: Use **Configuration Providers** (e.g., `appsettings.json`, Environment Variables, or Secrets Managers).

#### 3. Performance & Stability: Returning SqlDataReader 💣
- **Issue**: Returning `Ok(reader)` directly. `SqlDataReader` relies on an open connection.
- **Risk**: 
    - **Connection Leaks**: The `using` block disposes the connection when the method exits, which might happen before the data is fully serialized/sent.
    - **Serialization Errors**: Serializing a raw Reader is inefficient and prone to runtime errors.
- **Fix**: Map the result to a **DTO (Data Transfer Object)** while the connection is open, then return the DTO.

### ✅ Refactored Solution (Best Practice)

```csharp
[HttpGet("get-order/{orderId}")]
public async Task<IActionResult> GetOrder(string orderId)
{
    // 1. Fix: Retrieve connection string from Configuration (Env Vars / Secrets)
    string connString = _configuration.GetConnectionString("DefaultConnection");

    using (var conn = new SqlConnection(connString))
    {
        await conn.OpenAsync();

        // 2. Fix: Use Parameterized Query to prevent SQL Injection
        string sql = "SELECT * FROM Orders WHERE OrderId = @Id";

        // 3. Fix: Map to DTO/Entity using Dapper (or ADO.NET) and return the object
        var order = await conn.QuerySingleOrDefaultAsync<Order>(sql, new { Id = orderId });

        if (order == null) return NotFound();
        return Ok(order);
    }
}
```
