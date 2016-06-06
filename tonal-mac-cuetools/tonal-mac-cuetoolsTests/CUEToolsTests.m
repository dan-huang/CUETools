//
//  CUEToolsTests.m
//  tonal-mac-cuetoolsTests
//
//  Created by hjr on 6/20/16.
//  Copyright © 2016 icmd. All rights reserved.
//

#import <XCTest/XCTest.h>
#import "CUETools.h"

@interface CUEToolsTests : XCTestCase

@end
    
@implementation CUEToolsTests

- (void)testCUEToolsPerfect {
    XCTestExpectation *expectation = [self expectationWithDescription:@""];

    [CUETools repair:[NSString stringWithFormat:@"%@/TestFiles/Perfect/CDImage.cue", PROJECT_DIR]
          onProgress:^(CUEToolsStep step, double percent) {
              NSLog(@"step %i, per:%f", step, percent);
          } onEnd:^(bool exist, int confidence, int offset, NSString *binPath) {
              XCTAssertTrue(exist);
              XCTAssertGreaterThanOrEqual(confidence, 59);
              XCTAssertNil(binPath);
              [expectation fulfill];
          } onError:^(NSString *error) {
              [expectation fulfill];
          }];

    [self waitForExpectationsWithTimeout:INT16_MAX handler:^(NSError *error) {
    }];
}

- (void)testCUEToolsCanRepair {
    XCTestExpectation *expectation = [self expectationWithDescription:@""];

    [CUETools repair:[NSString stringWithFormat:@"%@/TestFiles/CanRepair/CDImage.cue", PROJECT_DIR]
          onProgress:^(CUEToolsStep step, double percent) {
              NSLog(@"ste:%i, per:%f", step, percent);
          } onEnd:^(bool exist, int confidence, int offset, NSString *binPath) {
              XCTAssertTrue(exist);
              XCTAssertGreaterThanOrEqual(confidence, 59);
              XCTAssertNotNil(binPath);
              [expectation fulfill];
          } onError:^(NSString *error) {
              [expectation fulfill];
          }];

    [self waitForExpectationsWithTimeout:INT16_MAX handler:^(NSError *error) {
    }];
}

- (void)testCUEToolsCannotRepair {
    XCTestExpectation *expectation = [self expectationWithDescription:@""];

    [CUETools repair:[NSString stringWithFormat:@"%@/TestFiles/CannotRepair/CDImage.cue", PROJECT_DIR]
          onProgress:^(CUEToolsStep step, double percent) {
          } onEnd:^(bool exist, int confidence, int offset, NSString *binPath) {
              XCTAssertTrue(exist);
              XCTAssertEqual(confidence, 0);
              XCTAssertEqual(offset, 0);
              XCTAssertNil(binPath);
              [expectation fulfill];
          } onError:^(NSString *error) {
              [expectation fulfill];
          }];

    [self waitForExpectationsWithTimeout:INT16_MAX handler:^(NSError *error) {
    }];
}

- (void)testCUEToolsNoRecordInDatabase {
    XCTestExpectation *expectation = [self expectationWithDescription:@""];

    [CUETools repair:[NSString stringWithFormat:
                      @"%@/TestFiles/NoRecordInDatabase/Impurfekt.-.[Ashes].cue", PROJECT_DIR]
          onProgress:^(CUEToolsStep step, double percent) {
          } onEnd:^(bool exist, int confidence, int offset, NSString *binPath) {
              XCTAssertFalse(exist);
              XCTAssertEqual(confidence, 0);
              XCTAssertEqual(offset, 0);
              XCTAssertNil(binPath);
              [expectation fulfill];
          } onError:^(NSString *error) {
              [expectation fulfill];
          }];

    [self waitForExpectationsWithTimeout:INT16_MAX handler:^(NSError *error) {
    }];
}

@end
