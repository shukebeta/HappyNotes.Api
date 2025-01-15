### HappyNotes Deployment Environments

1. **Production Environment**: Deployed by tags, allowing the frontend and backend to have different tags.  
   - **Frontend**: [https://happynotes.shukebeta.com](https://happynotes.shukebeta.com) or [https://happynotes.today](https://happynotes.today)  
   - **Backend**:  
     - [https://happynotes-api.shukebeta.com](https://happynotes-api.shukebeta.com) (RackNerd)  
     - [https://happynotes-img-uploader.shukebeta.com](https://happynotes-img-uploader.shukebeta.com) (Azure SS3)  
     - [https://happynotes-img.shukebeta.com](https://happynotes-img.shukebeta.com) (Azure SS3)  

2. **Beta Environment**: Deployed from the `master` branch. The frontend and backend always use the latest `master` code， and the backend connects to the production database.  
   - **Frontend**: [https://beta-happynotes.shukebeta.com](https://beta-happynotes.shukebeta.com)  
   - **Backend**:  
     - [https://beta-happynotes-api.shukebeta.com](https://beta-happynotes-api.shukebeta.com) (RackNerd)  
     - [https://happynotes-img-uploader.shukebeta.com](https://happynotes-img-uploader.shukebeta.com) (Azure SS3)  
     - [https://happynotes-img.shukebeta.com](https://happynotes-img.shukebeta.com) (Azure SS3)  

3. **Staging Environment**: Both the frontend and backend use the `master` code but connect to a staging database (not live).  
   - **Frontend**: [https://staging-happynotes.shukebeta.com](https://staging-happynotes.shukebeta.com)  
   - **Backend**:  
     - [https://staging-happynotes-api.shukebeta.com](https://staging-happynotes-api.shukebeta.com) (ARM)  
     - [https://staging-happynotes-img-uploader.shukebeta.com](https://staging-happynotes-img-uploader.shukebeta.com) (ARM)  
     - [https://staging-happynotes-img.shukebeta.com](https://staging-happynotes-img.shukebeta.com) (ARM)  

4. **Development Environment**: Both the frontend and backend use local code.  
   - **Frontend**: Runs local code and can access the local API, staging API, beta API, or production API based on the `.env` configuration.  
     - [http://xps.shukebeta.eu.org:49430](http://xps.shukebeta.eu.org:49430)  
   - **Backend**: Runs local code and should primarily connect to the local database to avoid conflicts.  
     - [https://zhw-happynotes-api.dev.shukebeta.com](https://zhw-happynotes-api.dev.shukebeta.com) (Local - XPS)  
     - [https://zhw-happynotes-img-uploader.dev.shukebeta.com](https://zhw-happynotes-img-uploader.dev.shukebeta.com) (Local - XPS)  
     - [https://zhw-happynotes-img.dev.shukebeta.com](https://zhw-happynotes-img.dev.shukebeta.com) (Local - XPS)  