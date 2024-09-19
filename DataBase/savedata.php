<?php
    $con = mysqli_connect('localhost', 'root', 'root', '3D_Connect_4');

    if (mysqli_connect_errno()) {
        echo "1: Connection failed"; // error code #1 = connection failed
        exit();
    }

    // Assuming IDs are passed directly through POST
    $p1_id = mysqli_real_escape_string($con, $_POST["p1_id"]);
    $p2_id = mysqli_real_escape_string($con, $_POST["p2_id"]);
    $winner_id = mysqli_real_escape_string($con, $_POST["winner_id"]);
    $moves = mysqli_real_escape_string($con, $_POST["moves"]);

    // Begin transaction
    mysqli_begin_transaction($con);

    try {
        // Insert into Session table
        $insertSessionQuery = $con->prepare("INSERT INTO Session (WinnerID, Moves) VALUES (?, ?)");
        $insertSessionQuery->bind_param("is", $winner_id, $moves);
        $insertSessionQuery->execute();
        $session_id = $con->insert_id;

        // Insert into Game_UserID for P1
        $insertJunction1Query = $con->prepare("INSERT INTO Game_UserID (UserID, SessionID, Colour) VALUES (?, ?, 1)");
        $insertJunction1Query->bind_param("ii", $p1_id, $session_id);
        $insertJunction1Query->execute();

        // Insert into Game_UserID for P2
        $insertJunction2Query = $con->prepare("INSERT INTO Game_UserID (UserID, SessionID) VALUES (?, ?, 2)");
        $insertJunction2Query->bind_param("ii", $p2_id, $session_id);
        $insertJunction2Query->execute();

        // Determine the loser ID
        $loser_id = ($winner_id == $p1_id) ? $p2_id : $p1_id;

        // Insert into Transaction table for Winner
        $pointsChange = -10;
        $reason = 'W'; // Assuming 'W' for Win
        $insertTransactionWinnerQuery = $con->prepare("INSERT INTO Transaction (SessionID, UserID, PointChange, Reason) VALUES (?, ?, ?, ?)");
        $insertTransactionWinnerQuery->bind_param("iiis", $session_id, $winner_id, $pointsChange, $reason);
        $insertTransactionWinnerQuery->execute();

        // Insert into Transaction table for Loser
        $pointsChange = 10;
        $reason = 'L'; // Assuming 'L' for Loss
        $insertTransactionLoserQuery = $con->prepare("INSERT INTO Transaction (SessionID, UserID, PointChange, Reason) VALUES (?, ?, ?, ?)");
        $insertTransactionLoserQuery->bind_param("iiis", $session_id, $loser_id, $pointsChange, $reason);
        $insertTransactionLoserQuery->execute();

        // Commit transaction
        mysqli_commit($con);
        echo "0"; // Success
    } catch (mysqli_sql_exception $exception) {
        mysqli_rollback($con);
        echo "3: Save game info failed - " . $exception->getMessage();
    }

    // Close all statement and connection
    $insertSessionQuery->close();
    $insertJunction1Query->close();
    $insertJunction2Query->close();
    $insertTransactionWinnerQuery->close();
    $insertTransactionLoserQuery->close();
    $con->close();
?>
