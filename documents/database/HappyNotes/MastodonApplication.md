# Database: HappyNotes Table: MastodonApplication

 Field         | Type         | Null | Default | Comment
---------------|--------------|------|---------|---------
 Id            | int          | NO   |         |
 InstanceUrl   | varchar(255) | NO   |         |
 ApplicationId | int          | NO   |         |
 ClientId      | varchar(255) | NO   |         |
 ClientSecret  | varchar(255) | NO   |         |
 MaxTootChars  | int          | NO   | 500     |
 Name          | varchar(255) | NO   |         |
 Website       | varchar(255) | YES  |         |
 RedirectUri   | varchar(255) | NO   |         |
 Scopes        | varchar(255) | NO   |         |
 CreatedAt     | bigint       | NO   |         |
 UpdatedAt     | bigint       | NO   |         |

## Indexes: 

 Key_name    | Column_name | Seq_in_index | Non_unique | Index_type | Visible
-------------|-------------|--------------|------------|------------|---------
 PRIMARY     | Id          |            1 |          0 | BTREE      | YES
 InstanceUrl | InstanceUrl |            1 |          0 | BTREE      | YES
