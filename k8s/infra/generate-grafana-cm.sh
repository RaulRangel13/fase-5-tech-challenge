kubectl create configmap grafana-datasources-cm --from-file=../../grafana/provisioning/datasources/datasource.yml --dry-run=client -o yaml > grafana-configmaps.yaml
kubectl create configmap grafana-dashboards-provider-cm --from-file=../../grafana/provisioning/dashboards/dashboard-provider.yml --dry-run=client -o yaml >> grafana-configmaps.yaml
kubectl create configmap grafana-dashboards-json-cm --from-file=../../grafana/dashboards/dashboard.json --dry-run=client -o yaml >> grafana-configmaps.yaml
