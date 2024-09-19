<?php
    $con = mysqli_connect('localhost', 'root', 'root', '3D_Connect_4');

    //check that connection happened
    if(mysqli_connect_errno()){
        echo "1: connection failed"; // 1 = connection failed
        exit();
    }

    $username = $_POST["name"];
    $password = $_POST["password"];

    //Check duplicates name
    $namecheckquery = "SELECT username FROM UserID WHERE username = '" . $username . "';";

    $namecheck = mysqli_query($con, $namecheckquery) or die("2: name check query failed "); //2 = name check query failed 

    if (mysqli_num_rows($namecheck) > 0){
        echo "3: name duplication"; //name duplication
        exit();
    }

    //add user to table // check on the security do somemore research
    $salt = "\$5\$rounds=5000\$"."bao".$username."\$";
    $hash = crypt($password, $salt);
    $insertuserquery = "INSERT INTO UserID (username, hash ,salt) VALUES ('".$username."','".$hash."','".$salt."');";
    mysqli_query($con, $insertuserquery) or die("4: Insert Player query failed"); //Insert Player query failed

    echo("0");

?>  