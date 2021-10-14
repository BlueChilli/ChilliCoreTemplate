# PDF generation Lambda 

This uses the `serverless` framework to deploy to AWS API Gateway and Lambda.

To install `serverless` run the following command - more details at https://serverless.com/framework/docs/providers/aws/guide/quick-start/:

`npm install -g serverless`

Install the AWS CLI (if it's not installed yet - https://aws.amazon.com/cli/)

To setup the AWS credentials used for deployment, you need to create a AWS IAM user with Administrator role
or you can follow the guide at https://serverless.com/framework/docs/providers/aws/guide/credentials/,
then run the command:

`aws configure`

Once complete, type `npm install`.

Once successfully set up, then `serverless deploy` will deploy an Api Endpoint and generate an API Key .

The last step is updating the configuration file for the desired environment:

        "PdfService": {
            "Uri": "",
            "ApiKey": ""
        }

--------------------------------------------------------------------------

See https://wkhtmltopdf.org/usage/wkhtmltopdf.txt for a complete list of available options when calling the pdf generation service.
Note that they need to be cammel-cased because of the nodejs component in use: .e.g { pageSize = "A4" } , for option --page-size

Sample code from `TestChartPdf` method in `ServerController` class:

        var pdfContent = await svc.GeneratePdfAsync(googleChartsSampleHtml, new
        {
            pageSize = "A4",
            orientation = "Portrait",
            marginTop = 0,
            marginRight = 0,
            marginBottom = 0,
            marginLeft = 0
        });
        
