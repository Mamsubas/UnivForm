SET QUOTED_IDENTIFIER ON;
SET ARITHABORT ON;

-- Fix Admin EmailConfirmed flag
UPDATE [AspNetUsers] 
SET [EmailConfirmed] = 1 
WHERE [Email] = 'admin@univform.com';

SELECT @@ROWCOUNT as [RowsUpdated];
