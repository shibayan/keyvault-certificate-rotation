# keyvault-certificate-rotation

![Build](https://github.com/shibayan/keyvault-certificate-rotation/workflows/Build/badge.svg)
[![Release](https://img.shields.io/github/release/shibayan/keyvault-certificate-rotation.svg)](https://github.com/shibayan/keyvault-certificate-rotation/releases/latest)
[![License](https://img.shields.io/github/license/shibayan/keyvault-certificate-rotation.svg)](https://github.com/shibayan/keyvault-certificate-rotation/blob/master/LICENSE)

This application provides automatic updating of the Key Vault Certificate for Azure CDN / Front Door.

Simply set up an IAM to the Azure Key Vault and Azure CDN / Front Door where the certificate is stored, and it will be updated to the new version of the certificate within 24 hours.

## Requirements

You will need the following:
- Azure Subscription (required to deploy this solution)
- Azure Key Vault
- Azure CDN / Front Door (pre-set up Key Vault certificates)

## Getting Started

### 1. Deploy Application

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fshibayan%2Fkeyvault-certificate-rotation%2Fmaster%2Fazuredeploy.json" target="_blank">
  <img src="https://azuredeploy.net/deploybutton.png" />
</a>


### 2. Add access control (IAM) to Azure CDN / Front Door

Open the `Access Control (IAM)` of the target CDN Profile / Front Door or resource group containing the CDN Profile / Front Door, and assign the role of `Contributor` to the deployed application.

### 3. Add to Key Vault access policies

Open the access policy of the Key Vault and add the `Certificate management` access policy for the deployed application.

## License

This project is licensed under the [Apache License 2.0](https://github.com/shibayan/keyvault-certificate-rotation/blob/master/LICENSE)
