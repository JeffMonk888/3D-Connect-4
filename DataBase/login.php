<?php
    $con = mysqli_connect('localhost', 'root', 'root', '3D_Connect_4');

    // Check that connection happened
    if (mysqli_connect_errno()) {
        echo "1: Connection Failed"; // error code #1 = connection failed
        exit();
    }

    $username = mysqli_real_escape_string($con, $_POST["name"]);
    $password = mysqli_real_escape_string($con, $_POST["password"]);

    // Check for user's existence
    $namecheckquery = "SELECT id, salt, hash FROM UserID WHERE username = '$username';";

    $namecheck = mysqli_query($con, $namecheckquery) or die("2: Name check query failed"); // error code #2 = name check query failed 

    if (mysqli_num_rows($namecheck) != 1) {
        echo "5: Either no user with name, or more than one"; // error code #5 = number of name matching != 1
        exit();
    }

    // Get login info from query
    $existing_info = mysqli_fetch_assoc($namecheck);
    $salt = $existing_info["salt"];
    $hash = $existing_info["hash"];

    $loginhash = crypt($password, $salt);
    if ($hash != $loginhash) {
        echo "6: Incorrect Password"; // error code #6 = incorrect Password
        exit();
    }

    // User is authenticated at this point, so let's get their score
    $userId = $existing_info["id"];

    // Calculate the user's score based on transaction history
    $scorequery = "SELECT SUM(PointChange) AS points_sum FROM Transaction WHERE UserID = '$userId';";
    $scoreresult = mysqli_query($con, $scorequery) or die("7: Score query failed"); // error code #7 = score query failed

    $row = mysqli_fetch_assoc($scoreresult);
    $pointsSum = $row['points_sum'] ?? 0; // Use null coalescing operator to handle NULL result
    $baseScore = 1000;
    $totalScore = $baseScore + $pointsSum;

    echo "0". "\t". $totalScore."\t". $userId;
?>
