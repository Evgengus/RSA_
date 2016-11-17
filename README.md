# RSA_

## Encrypt with create new keys

  1. **RSA.exe** get input data from the **Alice.txt**
  
  2. Create new keys
    
      * open key always have length 2 bytes
      
      * secret key's length given from argument command line in bits
      
      * general part keys have length equal secret key's length
  
  3. Encrypt( **_m^e (mod n)_** )
  
  4. Write output encrypted data to **Bob.txt**
  
  5. Secret key + general part key saved in **secret_key.txt** 
  
  6. Open key + general part key saved in **open_key.txt**
 
 
## Encrypt without create new keys

  1. **RSA.exe** get input data from the **Alice.txt**
  
  2. **RSA.exe** get open key and  general part key from **open_key.txt**
   
  2. Encrypt( **_m^e (mod n)_** )
  
  3. Write output encrypted data to **Bob.txt**
  
## Decrypt

  1. **RSA.exe** get input data from the **Bob.txt**
  
  2. **RSA.exe** get secret key and general part key from **secret_key.txt**
  
  3. Decrypt( **_c^d (mod n)_** )
  
  3. Write output encrypted data to **Alice.txt**
  
